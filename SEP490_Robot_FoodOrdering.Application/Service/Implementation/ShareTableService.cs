
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using Microsoft.Extensions.Options;
using SEP490_Robot_FoodOrdering.Application.Abstractions.ServerEndPoint;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Core.Ultils;
using AutoMapper;


namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ShareTableService 
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServerEndpointService _endpointService;
        private readonly IUtilsService _utils;
        private readonly IMapper _mapper;

        public ShareTableService(IUnitOfWork unitOfWork , IServerEndpointService endpointService , IUtilsService utils  , IMapper mapper )
        {
            _unitOfWork = unitOfWork;
            _endpointService = endpointService;
            _utils = utils;
            _mapper = mapper;
        }

        public async Task<BaseResponseModel<TableResponse>> AcceptSharedTableAsync(Guid tableId, string shareToken, string newDeviceId)
        {
            var table = _unitOfWork.Repository<Table, Guid>().GetWithSpecAsync(new BaseSpecification<Table>(x => x.Id == tableId && x.ShareToken == shareToken && x.isShared == true));
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Không tìm thấy bàn hoặc token không hợp lệ");
            if (table.Result.LockedAt == null || table.Result.LockedAt.Value.AddMinutes(15) < DateTime.UtcNow)
            {
                throw new ErrorException(StatusCodes.Status403Forbidden, ResponseCodeConstants.FORBIDDEN, "Token đã hết hạn");
            }
            else
            {
                table.Result.DeviceId = newDeviceId;
                table.Result.isShared = false;
                table.Result.ShareToken = null;
                table.Result.LastAccessedAt = DateTime.UtcNow;
                table.Result.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<Table, Guid>().Update(table.Result);
                await _unitOfWork.SaveChangesAsync();
                return (new BaseResponseModel<TableResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, _mapper.Map<TableResponse>(table.Result), null, "Chấp nhận chia sẻ bàn thành công"));
            }
        }

        public async Task<BaseResponseModel<QrShareResponse>> TableShareAsync(Guid tableId, string CurrentDevideId)
        {
            var table = await _unitOfWork.Repository<Table, Guid>().GetWithSpecAsync(new BaseSpecification<Table>(x => x.Id == tableId && x.DeviceId == CurrentDevideId));
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Không tìm thấy người dùng hiện tại ở bàn {table.Name} ");
            var sharetoken = Guid.NewGuid().ToString("N");

            table.ShareToken = sharetoken;
            table.isShared = true;
            table.LockedAt = DateTime.UtcNow;
            table.LastAccessedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            var shareUrl = _endpointService.GetBackendUrl() + $"/TableShare/{tableId}/accept-share?shareToken={sharetoken}?newDeviceId=";
         
            var data = new QrShareResponse
            {
                ShareToken = sharetoken,
                ShareUrl = shareUrl,
                ExpireAt = DateTime.UtcNow.AddMinutes(15)
            };

            return new BaseResponseModel<QrShareResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, data, null, "Chia sẻ bàn thành công,");
        }

    }
}
