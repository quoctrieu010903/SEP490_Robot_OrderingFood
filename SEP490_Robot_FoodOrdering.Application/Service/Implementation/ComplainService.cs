

using AutoMapper;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;
using SEP490_Robot_FoodOrdering.Application.DTO.Response;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
using SEP490_Robot_FoodOrdering.Domain.Specifications;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ComplainService : IComplainService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOrderStatsQuery _orderStatsService;
        private readonly IModeratorDashboardRefresher _moderatorDashboardRefresher;

        private readonly IHttpContextAccessor _httpContextAccessor;
        public ComplainService(IUnitOfWork unitOfWork, IMapper mapper, IOrderStatsQuery orderStatsService , IModeratorDashboardRefresher moderatorDashboardRefresher , IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderStatsService = orderStatsService;
            _moderatorDashboardRefresher = moderatorDashboardRefresher;
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<BaseResponseModel<List<ComplainCreate>>> ComfirmComplain(
        Guid idTable,
        List<Guid>? IDFeedback,
        bool isPending,
        string content)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new ErrorException(StatusCodes.Status401Unauthorized, "UNAUTHORIZED",
                    "User is not authenticated.");
            }

            // 🔹 1️⃣ Lấy tất cả complain theo bàn
            var feedbackEntities = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecWithInclueAsync(
                    new BaseSpecification<Complain>(f => f.TableId == idTable),
                    true,
                    f=>f.Table
                );

            if (feedbackEntities == null || !feedbackEntities.Any())
                throw new ErrorException(404, "Không tìm thấy khiếu nại cho bàn này.");

            // 🔹 2️⃣ Xác định tập complain cần xử lý
            var targetFeedbacks = (IDFeedback == null || !IDFeedback.Any())
                ? feedbackEntities // Xử lý tất cả
                : feedbackEntities.Where(f => IDFeedback.Contains(f.Id)).ToList();

            if (!targetFeedbacks.Any())
                throw new ErrorException(404, "Không tìm thấy khiếu nại với các ID đã cho.");

            // 🔹 3️⃣ Cập nhật trạng thái từng complain
            var updatedFeedbacks = new List<ComplainCreate>();

            foreach (var feedback in targetFeedbacks)
            {
                // ✅ Không cần quan tâm có OrderItemId hay không
                feedback.isPending = isPending;
                feedback.ResolutionNote = content;
                feedback.ResolvedAt = DateTime.UtcNow;
                feedback.HandledBy = Guid.Parse(userIdClaim);

                await _unitOfWork.Repository<Complain, Guid>().UpdateAsync(feedback);

                // 🧩 Mapping ra DTO an toàn
                updatedFeedbacks.Add(new ComplainCreate(
                    feedback.CreatedTime,
                    feedback.isPending,
                    feedback.Description 
                   
                   
                ));
            }

            // 🔹 4️⃣ Lưu thay đổi
            await _unitOfWork.SaveChangesAsync();

            _moderatorDashboardRefresher.PushTableAsync(idTable);
            // 🔹 5️⃣ Trả kết quả
            return new BaseResponseModel<List<ComplainCreate>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                updatedFeedbacks
            );
        }







        public async Task<BaseResponseModel<ComplainCreate>> CreateComplainAsyns(ComplainRequests request)
        {
            // ✅ 1. Kiểm tra bàn có tồn tại không
            var existedTable = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(request.TableId);
            if (existedTable == null)
                throw new ErrorException(
                    StatusCodes.Status404NotFound,
                    ResponseCodeConstants.NOT_FOUND,
                    "Không tìm thấy bàn (table).");
            
            
          
                var complain = new Complain
                {
                    Id = Guid.NewGuid(),
                    TableId = request.TableId,
                    Title = request.Title,
                    Description = request.ComplainNote,
                    isPending = true, 
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Complain, Guid>().AddAsync(complain);
            
            // ✅ 4. Lưu thay đổi
            await _unitOfWork.SaveChangesAsync();

            // Gửi thông báo cập nhật dashboard cho moderator
            await _moderatorDashboardRefresher.PushTableAsync(request.TableId);

            // ✅ 5. Trả kết quả
            var response = new ComplainCreate(DateTime.UtcNow, true, "Tạo complain thành công");
            return new BaseResponseModel<ComplainCreate>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                response);
        }


        /*public async Task<BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>> GetAllComplainIsPending1()
        {
            // Lấy tất cả dữ liệu cần thiết
            var tables = await _unitOfWork.Repository<Table, Guid>().GetAllWithIncludeAsync(true , t=> t.Orders, t => t.Sessions);
            var complains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<Complain>(x => x.isPending));

            if (tables == null || !tables.Any())
                throw new ErrorException(404, "No tables found");

            // Lấy toàn bộ thống kê order cho các bàn
            var orderStatsDict = await _orderService.GetOrderStatsByTableIds(tables.Select(x => x.Id));

            // 🔹 Gộp dữ liệu bằng LINQ
            var result = tables.Select(table =>
            {
                //int pendingCount = complains.TryGetValue(table.Id, out var count) ? count : 0;
                int pendingCount = complains.Count(complains => complains.TableId == table.Id);
                var activeSession = table.Sessions.FirstOrDefault();
                var sessionId = activeSession?.Id.ToString() ?? string.Empty;

                DateTime? lastOrderUpdatedTime = table.Orders != null && table.Orders.Any()
                    ? table.Orders
                        .OrderByDescending(o => o.LastUpdatedTime)
                        .Select(o => (DateTime?)o.LastUpdatedTime)
                        .FirstOrDefault()
                    : null;

                var stats = (activeSession != null && orderStatsDict.TryGetValue(table.Id, out var s))
                    ? s
                    : new OrderStaticsResponse { PaymentStatus = 0, DeliveredCount = 0, ServedCount = 0, PaidCount = 0, TotalOrderItems = 0 };

                return new ComplainPeedingInfo(
                    Id: table.Id,
                    SessionId: sessionId,
                    TableName: table.Name,
                    tableStatus: table.Status,
                    paymentStatus: stats.PaymentStatus,
                    Counter: pendingCount,
                    DeliveredCount: stats.DeliveredCount,
                    ServeredCount: stats.ServedCount,
                    PaidCount: stats.PaidCount,
                    TotalItems: stats.TotalOrderItems,
                    LastOrderUpdatedTime: lastOrderUpdatedTime
                );
            }).ToDictionary(x => x.Id.ToString(), x => x);


            return new BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                result
            );
        }*/

        public async Task<BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>> GetAllComplainIsPending()
        {
            // 1) Load tables
            var tables = await _unitOfWork.Repository<Table, Guid>()
                .GetAllWithIncludeAsync(true, t => t.Orders, t => t.Sessions);

            if (tables == null || !tables.Any())
                throw new ErrorException(404, "No tables found");

            // 2) Get active sessions (chưa checkout)
            var activeSessions = await _unitOfWork.Repository<TableSession, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<TableSession>(s =>
                    s.CheckOut == null && s.Status == TableSessionStatus.Active
                ));

            // Nếu không có session active -> trả snapshot rỗng (tùy bạn)
            if (activeSessions == null || !activeSessions.Any())
            {
                var empty = new Dictionary<string, ComplainPeedingInfo>();
                await _moderatorDashboardRefresher.PushSnapshotAsync();

                return new BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    empty
                );
            }

            // 3) Map active session mới nhất theo TableId
            var activeSessionByTable = activeSessions
                .GroupBy(s => s.TableId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CheckIn).First());

            var activeTableIds = activeSessionByTable.Keys.ToHashSet();
            var activeSessionIds = activeSessions.Select(s => s.Id).ToHashSet();

            // 4) Lấy first orders của tất cả session active (1 query)
            var ordersInActiveSessions = await _unitOfWork.Repository<Order, Guid>()
                .GetAllWithSpecAsync(new FirstOrderInSessionsSpec(activeSessionIds));

            // Map sessionId -> firstOrderCreatedTime
            var firstOrderTimeBySession = ordersInActiveSessions
                .Where(o => o.TableSessionId.HasValue)
                .GroupBy(o => o.TableSessionId!.Value)
                .ToDictionary(g => g.Key, g => g.Min(x => x.CreatedTime));

            // 5) Tính sessionStart cho từng TableId (đồng bộ với API chi tiết)
            // sessionStart = firstOrderCreatedTime ?? activeSession.CheckIn
            var sessionStartByTable = activeSessionByTable.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var session = kvp.Value;
                    if (firstOrderTimeBySession.TryGetValue(session.Id, out var tFirst))
                        return (DateTime?)tFirst;
                    return (DateTime?)session.CheckIn;
                }
            );

            // 6) Lấy complain pending của các bàn active (1 query)
            var pendingComplainsRaw = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<Complain>(c =>
                    c.isPending && activeTableIds.Contains(c.TableId)
                ));

            // 7) Lọc complain thuộc session hiện tại theo sessionStart (in-memory)
            var pendingComplains = pendingComplainsRaw
                .Where(c =>
                    sessionStartByTable.TryGetValue(c.TableId, out var start)
                    && start.HasValue
                    && c.CreatedTime >= start.Value
                )
                .ToList();

            // 8) Group count theo TableId để counter không lệch
            var pendingCountByTable = pendingComplains
                .GroupBy(c => c.TableId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 9) Stats (nên lấy theo activeTables cho nhẹ)
            var activeTables = tables.Where(t => activeTableIds.Contains(t.Id)).ToList();

            var orderStatsDict = await _orderStatsService
                .GetOrderStatsByTableIdsAsync(activeTables.Select(x => x.Id));

            // 10) Build result — (khuyến nghị chỉ trả về activeTables để đúng intent complain pending)
            var result = activeTables.Select(table =>
            {
                var activeSession = activeSessionByTable[table.Id];
                var sessionId = activeSession.Id.ToString();

                pendingCountByTable.TryGetValue(table.Id, out var pendingCount);

                DateTime? lastOrderUpdatedTime = table.Orders != null && table.Orders.Any()
                    ? table.Orders
                        .OrderByDescending(o => o.LastUpdatedTime)
                        .Select(o => (DateTime?)o.LastUpdatedTime)
                        .FirstOrDefault()
                    : null;

                var stats = new OrderStaticsResponse
                {
                    PaymentStatus = 0,
                    DeliveredCount = 0,
                    ServedCount = 0,
                    PaidCount = 0,
                    TotalOrderItems = 0
                };

                if (orderStatsDict.TryGetValue(table.Id, out var s))
                    stats = s;

                var pendingItems = Math.Max(0, stats.TotalOrderItems - stats.ServedCount);

                bool isWaitingDish = pendingItems > 0 && table.Status == TableEnums.Occupied;

                int? waitingDurationInMinutes = null;
                if (isWaitingDish && lastOrderUpdatedTime.HasValue)
                {
                    var now = DateTime.UtcNow;
                    waitingDurationInMinutes =
                        (int)Math.Floor((now - lastOrderUpdatedTime.Value).TotalMinutes);
                }

                return new ComplainPeedingInfo(
                    Id: table.Id,
                    SessionId: sessionId,
                    TableName: table.Name,
                    tableStatus: table.Status,
                    paymentStatus: stats.PaymentStatus,
                    Counter: pendingCount,
                    DeliveredCount: stats.DeliveredCount,
                    ServeredCount: stats.ServedCount,
                    PaidCount: stats.PaidCount,
                    TotalItems: stats.TotalOrderItems,
                    LastOrderUpdatedTime: lastOrderUpdatedTime,
                    PendingItems: pendingItems,
                    IsWaitingDish: isWaitingDish,
                    WaitingDurationInMinutes: waitingDurationInMinutes
                );
            }).ToDictionary(x => x.Id.ToString(), x => x);

            await _moderatorDashboardRefresher.PushSnapshotAsync();

            return new BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                result
            );
        }



        public async Task<BaseResponseModel<List<ComplainResponse>>> GetComplainByTable(
      Guid idTable,
      bool forCustomer = false
  )
        {
            DateTime? sessionStart = null;
            DateTime? sessionEnd = null;

            if (forCustomer)
            {
                var activeSession = await _unitOfWork.Repository<TableSession, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<TableSession>(s =>
                        s.TableId == idTable
                        && s.CheckOut == null
                        && s.Status == TableSessionStatus.Active
                    ));

                // ✅ Không có session => coi như không có complain trong session hiện tại
                if (activeSession == null)
                {
                    return new BaseResponseModel<List<ComplainResponse>>(
                        StatusCodes.Status200OK,
                        ResponseCodeConstants.SUCCESS,
                        new List<ComplainResponse>()
                    );
                }

                var firstOrder = await _unitOfWork.Repository<Order, Guid>()
                    .GetWithSpecAsync(new FirstOrderInSessionSpec(idTable, activeSession.Id));

                sessionStart = firstOrder?.CreatedTime ?? activeSession.CheckIn;
                sessionEnd = activeSession.CheckOut; // đang null vì active session, nhưng vẫn giữ logic
            }

            var spec = new BaseSpecification<Complain>(c =>
                c.TableId == idTable
                && (
                    !forCustomer
                    || (
                        sessionStart.HasValue
                        && c.CreatedTime >= sessionStart.Value
                        && (!sessionEnd.HasValue || c.CreatedTime <= sessionEnd.Value)
                    )
                )
            );

            var complains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecWithInclueAsync(spec, true, c => c.Handler);

            // ✅ Tuỳ bạn: forCustomer thì có thể trả list rỗng thay vì 404
            if (complains == null || !complains.Any())
            {
                return new BaseResponseModel<List<ComplainResponse>>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    new List<ComplainResponse>()
                );
            }

            var responseList = complains
                .OrderByDescending(c => c.CreatedTime)
                .Select(c => new ComplainResponse
                {
                    ComplainId = c.Id,
                    IdTable = c.TableId,
                    FeedBack = c.Description,
                    CreateData = c.CreatedTime,
                    IsPending = c.isPending,
                    ResolutionNote = c.ResolutionNote,
                    HandledBy = c.Handler != null ? c.Handler.FullName : null
                })
                .ToList();

            return new BaseResponseModel<List<ComplainResponse>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                responseList
            );
        }




    }
}
