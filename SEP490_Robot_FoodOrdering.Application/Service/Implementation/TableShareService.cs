using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
using SEP490_Robot_FoodOrdering.Application.Abstractions.ServerEndPoint;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class TableShareService : ITableShare
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITableActivityService _tableActivityService;
        private readonly IModeratorDashboardRefresher _moderatorDashboardRefresher;
        private readonly IServerEndpointService _endpointService;
        private readonly IUtilsService _utils;
        private readonly ILogger<TableShareService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TableShareService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ITableActivityService tableActivityService,
            IModeratorDashboardRefresher moderatorDashboardRefresher,
            IServerEndpointService endpointService,
            IUtilsService utils,
            ILogger<TableShareService> logger , IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tableActivityService = tableActivityService;
            _moderatorDashboardRefresher = moderatorDashboardRefresher;
            _endpointService = endpointService;
            _utils = utils;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResponseModel<TableResponse>> AttachDeviceAsync(Guid tableId, AttachDeviceRequest request)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return new BaseResponseModel<TableResponse>(StatusCodes.Status401Unauthorized, "UNAUTHORIZED",
                    "User is not authenticated.");
            }

            var table = await _unitOfWork.Repository<Table, Guid>().GetByIdWithIncludeAsync(t => t.Id == tableId, true, t => t.Sessions);
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

            if (table.Status != TableEnums.Occupied)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_OPERATION, "Bàn phải ở trạng thái đang sử dụng mới có thể gán thiết bị");

            var activeSession = table.Sessions.OrderByDescending(s => s.CheckIn).FirstOrDefault(s => s.Status == TableSessionStatus.Active);
            if (activeSession == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Không tìm thấy session hoạt động cho bàn này");

            string oldDeviceId = table.DeviceId ?? "Unknown";

            // Update Table
            table.DeviceId = request.NewDeviceId;
            table.LastUpdatedTime = DateTime.UtcNow;
            table.LastUpdatedBy = userId.ToString();

            // Update Session
            activeSession.DeviceId = request.NewDeviceId;
            activeSession.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.Repository<Table, Guid>().Update(table);
            _unitOfWork.Repository<TableSession, Guid>().Update(activeSession);

            // Log activity
            var sessionWithOrders = await _unitOfWork.Repository<TableSession, Guid>().GetByIdWithIncludeAsync(s => s.Id == activeSession.Id, true, s => s.Orders);
            var orderCodes = sessionWithOrders?.Orders.Select(o => o.OrderCode).ToList();

            await _tableActivityService.LogAsync(
                activeSession,
                request.NewDeviceId,
                TableActivityType.AttachDeviceFromModerator,
                new
                {
                    tableName = table.Name,
                    orderCode = orderCodes != null && orderCodes.Any() ? string.Join(", ", orderCodes) : "No Order",
                    reason = request.Reason,
                    oldDeviceId = oldDeviceId,
                    newDeviceId = request.NewDeviceId,
                    moderatorAction = true
                }
            );

            await _unitOfWork.SaveChangesAsync();

            // Notify moderator dashboard
            await _moderatorDashboardRefresher.PushTableAsync(table.Id);

            return new BaseResponseModel<TableResponse>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                _mapper.Map<TableResponse>(table),
                null,
                "Gán thiết bị thành công"
            );
        }

        public async Task<BaseResponseModel<QrShareResponse>> ShareTableAsync(Guid tableId, string currentDeviceId)
        {
            var table = await _unitOfWork.Repository<Table, Guid>().GetWithSpecAsync(new BaseSpecification<Table>(x => x.Id == tableId && x.DeviceId == currentDeviceId));
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Không tìm thấy người dùng hiện tại ở bàn {table?.Name} ");

            var shareToken = Guid.NewGuid().ToString("N");

            table.ShareToken = shareToken;
            table.isShared = true;
            table.LockedAt = DateTime.UtcNow;
            table.LastAccessedAt = DateTime.UtcNow;
            
            _unitOfWork.Repository<Table, Guid>().Update(table);
            await _unitOfWork.SaveChangesAsync();

            var shareUrl = _endpointService.GetBackendUrl() + $"/api/TableShare/{tableId}/accept-share?shareToken={shareToken}&newDeviceId=";
            
            var data = new QrShareResponse
            {
                QrCodeBase64 = "data:image/png;base64," + _utils.GenerateQrCodeBase64_NoDrawing(shareUrl),
                ShareToken = shareToken,
                ShareUrl = shareUrl,
                ExpireAt = DateTime.UtcNow.AddMinutes(15)
            };

            return new BaseResponseModel<QrShareResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, data, null, "Chia sẻ bàn thành công");
        }

        public async Task<BaseResponseModel<TableResponse>> AcceptSharedTableAsync(Guid tableId, string shareToken, string newDeviceId)
        {
            var table = await _unitOfWork.Repository<Table, Guid>().GetWithSpecAsync(new BaseSpecification<Table>(x => x.Id == tableId && x.ShareToken == shareToken && x.isShared == true));
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Không tìm thấy bàn hoặc token không hợp lệ");

            if (table.LockedAt == null || table.LockedAt.Value.AddMinutes(15) < DateTime.UtcNow)
            {
                throw new ErrorException(StatusCodes.Status403Forbidden, ResponseCodeConstants.FORBIDDEN, "Token đã hết hạn");
            }

            table.DeviceId = newDeviceId;
            table.isShared = false;
            table.ShareToken = null;
            table.LastAccessedAt = DateTime.UtcNow;
            table.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.Repository<Table, Guid>().Update(table);
            
            // Also update the active session's deviceId if needed
            var activeSession = (await _unitOfWork.Repository<Table, Guid>().GetByIdWithIncludeAsync(t => t.Id == tableId, true, t => t.Sessions))
                ?.Sessions.OrderByDescending(s => s.CheckIn).FirstOrDefault(s => s.Status == TableSessionStatus.Active);
            
            if (activeSession != null)
            {
                activeSession.DeviceId = newDeviceId;
                activeSession.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<TableSession, Guid>().Update(activeSession);
                
                var sessionWithOrders = await _unitOfWork.Repository<TableSession, Guid>().GetByIdWithIncludeAsync(s => s.Id == activeSession.Id, true, s => s.Orders, s => s.Table);
                var orderCodes = sessionWithOrders?.Orders.Select(o => o.OrderCode).ToList();
                var tableName = sessionWithOrders?.Table?.Name ?? table.Name;

                await _tableActivityService.LogAsync(
                    activeSession,
                    newDeviceId,
                    TableActivityType.ShareJoin,
                    new 
                    { 
                        tableName = tableName,
                        orderCode = orderCodes != null && orderCodes.Any() ? string.Join(", ", orderCodes) : "No Order",
                        reason = "Chấp nhận chia sẻ bàn",
                        deviceId = newDeviceId 
                    }
                );
            }

            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel<TableResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, _mapper.Map<TableResponse>(table), null, "Chấp nhận chia sẻ bàn thành công");
        }

        public Task<BaseResponseModel<TableResponse>> TransferTableAsync(Guid tableId, Guid transferToUserId, string? reason = null, string transferredBy = "System")
        {
            throw new NotImplementedException();
        }
    }
}
