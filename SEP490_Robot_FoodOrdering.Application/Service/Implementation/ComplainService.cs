

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
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ComplainService : IComplainService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOrderStatsQuery _orderStatsService;
        private readonly IModeratorDashboardRefresher _moderatorDashboardRefresher;
        private readonly IAdminDashboardRefresher _adminDashboardRefresher;
        private readonly INotificationService _notificationService;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUtilsService _utilsService;
        public ComplainService(IUnitOfWork unitOfWork, IMapper mapper, IOrderStatsQuery orderStatsService , IModeratorDashboardRefresher moderatorDashboardRefresher, IAdminDashboardRefresher adminDashboardRefresher, IHttpContextAccessor httpContextAccessor, INotificationService notificationService, IUtilsService utilsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderStatsService = orderStatsService;
            _moderatorDashboardRefresher = moderatorDashboardRefresher;
            _adminDashboardRefresher = adminDashboardRefresher;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
            _utilsService = utilsService;

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

                // 🔹 Nhận diện request "Gửi nhanh" từ moderator dựa trên ResolutionNote
                var isQuickServeRequest = !string.IsNullOrWhiteSpace(content) &&
                                          content.StartsWith("Yêu cầu nhanh:", StringComparison.OrdinalIgnoreCase);

                // Nếu là yêu cầu phục vụ nhanh thì chuẩn hóa Title về "Phục vụ nhanh"
                if (isQuickServeRequest)
                {
                    feedback.Title = "Phục vụ nhanh";
                }

                // 🔹 Xử lý QuickServeItem cho các complain có Title = "Phục vụ nhanh"
                if (!string.IsNullOrWhiteSpace(feedback.Title) &&
                    feedback.Title.Equals("Phục vụ nhanh", StringComparison.OrdinalIgnoreCase) 
                    && !string.IsNullOrWhiteSpace(content))
                {
                    await ProcessQuickServeItemsAsync(feedback.Id, content);
                }

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

          await  _moderatorDashboardRefresher.PushTableAsync(idTable);
          
          // 🔹 Cập nhật Admin Dashboard (thống kê khiếu nại)
          await _adminDashboardRefresher.PushDashboardAsync();
          
            if (isPending && content.Contains("Yêu cầu nhanh:"))
            {
                try
                {
                    // Extract product name from content (e.g., "Yêu cầu nhanh: Cho thêm nước mắm" -> "Cho thêm nước mắm")
                    var productName = content.Replace("Yêu cầu nhanh:", "").Trim();
                    var notificationMessage = $"Có yêu cầu phục vụ nhanh: {productName}";
                    await _notificationService.SendWaiterNotificationAsync(notificationMessage, "QuickServeRequest");
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the request
                    // Could add logging here if needed
                }
            }
            
            // 🔹 5️⃣ Trả kết quả
            return new BaseResponseModel<List<ComplainCreate>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                updatedFeedbacks
            );
        }




        public async Task<BaseResponseModel<List<QuickServeItemResponse>>> GetPendingQuickServeItemsAsync()
        {
            // Lấy complain pending có Title = "Phục vụ nhanh" + include Table để có TableId, TableName
            var pendingQuickComplains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecWithInclueAsync(
                    new BaseSpecification<Complain>(c =>
                        c.isPending && c.Title == "Phục vụ nhanh"),
                    true,
                    c => c.Table
                );

            if (pendingQuickComplains == null || !pendingQuickComplains.Any())
            {
                return new BaseResponseModel<List<QuickServeItemResponse>>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    new List<QuickServeItemResponse>()
                );
            }

            var complainLookup = pendingQuickComplains.ToDictionary(c => c.Id, c => c);
            var complainIds = complainLookup.Keys.ToHashSet();

            var items = await _unitOfWork.Repository<QuickServeItem, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<QuickServeItem>(q =>
                    complainIds.Contains(q.ComplainId) && !q.IsServed));

            var response = items
                .Select(i =>
                {
                    var complain = complainLookup[i.ComplainId];
                    return new QuickServeItemResponse
                    {
                        Id = i.Id,
                        ComplainId = i.ComplainId,
                        TableId = complain.TableId,
                        TableName = complain.Table?.Name ?? string.Empty,
                        ItemName = i.ItemName,
                        IsServed = i.IsServed,
                        CreatedTime = i.CreatedTime,
                        LastUpdatedTime = i.LastUpdatedTime
                    };
                })
                .ToList();

            return new BaseResponseModel<List<QuickServeItemResponse>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                response
            );
        }

        /// <summary>
        /// Lấy tất cả QuickServeItems đã được phục vụ (IsServed = true) để show ở tab Đã phục vụ.
        /// </summary>
        public async Task<BaseResponseModel<List<QuickServeItemResponse>>> GetServedQuickServeItemsAsync()
        {
            // Lấy toàn bộ quick-serve items đã phục vụ
            var servedItems = await _unitOfWork.Repository<QuickServeItem, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<QuickServeItem>(q => q.IsServed));

            if (servedItems == null || !servedItems.Any())
            {
                return new BaseResponseModel<List<QuickServeItemResponse>>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    new List<QuickServeItemResponse>()
                );
            }

            // Lấy complain + table info để map TableId/TableName
            var complainIds = servedItems.Select(i => i.ComplainId).Distinct().ToHashSet();
            var relatedComplains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecWithInclueAsync(
                    new BaseSpecification<Complain>(c => complainIds.Contains(c.Id)),
                    true,
                    c => c.Table
                );
            var complainLookup = relatedComplains.ToDictionary(c => c.Id, c => c);

            var response = servedItems.Select(i =>
            {
                complainLookup.TryGetValue(i.ComplainId, out var complain);
                return new QuickServeItemResponse
                {
                    Id = i.Id,
                    ComplainId = i.ComplainId,
                    TableId = complain?.TableId ?? Guid.Empty,
                    TableName = complain?.Table?.Name ?? string.Empty,
                    ItemName = i.ItemName,
                    IsServed = i.IsServed,
                    CreatedTime = i.CreatedTime,
                    LastUpdatedTime = i.LastUpdatedTime
                };
            }).ToList();

            return new BaseResponseModel<List<QuickServeItemResponse>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                response
            );
        }

        /// <summary>
        /// Đánh dấu 1 quick-serve item đã được phục vụ.
        /// Nếu tất cả item của complain đã IsServed = true thì set complain.isPending = false.
        /// </summary>
        public async Task<BaseResponseModel<bool>> ServeQuickServeItemAsync(Guid quickServeItemId)
        {
            var itemRepo = _unitOfWork.Repository<QuickServeItem, Guid>();
            var complainRepo = _unitOfWork.Repository<Complain, Guid>();

            var item = await itemRepo.GetByIdAsync(quickServeItemId);
            if (item == null)
            {
                throw new ErrorException(404, "Không tìm thấy yêu cầu phục vụ nhanh.");
            }

            if (!item.IsServed)
            {
                item.IsServed = true;
                item.LastUpdatedTime = DateTime.UtcNow;
                await itemRepo.UpdateAsync(item);
                // Flush ngay lập tức để các request song song nhìn thấy trạng thái mới nhất
                await _unitOfWork.SaveChangesAsync();
            }

            // Kiểm tra sau khi đã flush DB để tránh race-condition khi phục vụ nhiều món cùng lúc
            var hasUnservedItems = await itemRepo.AnyAsync(
                new BaseSpecification<QuickServeItem>(q =>
                    q.ComplainId == item.ComplainId && !q.IsServed));

            if (!hasUnservedItems)
            {
                var complain = await complainRepo.GetByIdAsync(item.ComplainId);
                if (complain != null && complain.isPending)
                {
                    complain.isPending = false;
                    complain.ResolvedAt = DateTime.UtcNow;
                    await complainRepo.UpdateAsync(complain);
                    await _unitOfWork.SaveChangesAsync();
                    
                    // 🔹 Cập nhật Admin Dashboard (khiếu nại đã được xử lý hoàn tất)
                    await _adminDashboardRefresher.PushDashboardAsync();
                }
            }

            return new BaseResponseModel<bool>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                true
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
            
            // 🔹 Cập nhật Admin Dashboard (thống kê khiếu nại mới)
            await _adminDashboardRefresher.PushDashboardAsync();

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
            // 1) Load ALL tables (giữ nguyên để grid hiện đủ)
            var tables = await _unitOfWork.Repository<Table, Guid>()
                .GetAllWithIncludeAsync(true, t => t.Orders, t => t.Sessions);

            if (tables == null || !tables.Any())
                throw new ErrorException(404, "No tables found");

            // 2) Active sessions
            var activeSessions = await _unitOfWork.Repository<TableSession, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<TableSession>(s =>
                    s.CheckOut == null && s.Status == TableSessionStatus.Active
                ));

            // Nếu không có session active -> vẫn trả ALL tables nhưng counter = 0
            if (activeSessions == null || !activeSessions.Any())
            {
                var emptyResult = tables.Select(table => new ComplainPeedingInfo(
                    Id: table.Id,
                    SessionId: "",
                    TableName: table.Name,
                    tableStatus: table.Status,
                    paymentStatus: 0,
                    Counter: 0,
                    DeliveredCount: 0,
                    ServeredCount: 0,
                    PaidCount: 0,
                    TotalItems: 0,
                    LastOrderUpdatedTime: null,
                    PendingItems: 0,
                    IsWaitingDish: false,
                    WaitingDurationInMinutes: null,
                    ListComplain: null
                )).ToDictionary(x => x.Id.ToString(), x => x);

                await _moderatorDashboardRefresher.PushSnapshotAsync();

                return new BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    emptyResult
                );
            }

            // 3) Map active session mới nhất theo TableId
            var activeSessionByTable = activeSessions
                .GroupBy(s => s.TableId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CheckIn).First());

            var activeTableIds = activeSessionByTable.Keys.ToHashSet();
            var activeSessionIds = activeSessions.Select(s => s.Id).ToHashSet();

            // 4) Lấy first orders của tất cả active sessions (1 query)
            var ordersInActiveSessions = await _unitOfWork.Repository<Order, Guid>()
                .GetAllWithSpecAsync(new FirstOrderInSessionsSpec(activeSessionIds));

            var firstOrderTimeBySession = ordersInActiveSessions
                .Where(o => o.TableSessionId.HasValue)
                .GroupBy(o => o.TableSessionId!.Value)
                .ToDictionary(g => g.Key, g => g.Min(x => x.CreatedTime));

            // 5) sessionStart theo TableId (first order time ?? checkin)
            var sessionStartByTable = activeSessionByTable.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var session = kvp.Value;
                    return firstOrderTimeBySession.TryGetValue(session.Id, out var tFirst)
                        ? (DateTime?)tFirst
                        : (DateTime?)session.CheckIn;
                });

            // 6) lấy pending complains của active tables
            var pendingComplainsRaw = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<Complain>(c =>
                    c.isPending && activeTableIds.Contains(c.TableId)
                ));

            // 8) Group pending complains by tableId
            var complaintsByTable = pendingComplainsRaw
                .GroupBy(c => c.TableId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => new ComplainItemInfo(
                        c.Id,
                        c.Description,
                        c.Title,
                        c.CreatedTime,
                        _utilsService.ParseQuickServeItems(c.Description ?? "")
                    )).ToList()
                );

            var pendingCountByTable = complaintsByTable.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

            // 9) order stats (bạn có thể lấy cho ALL tables hoặc chỉ activeTables)
            // Nếu muốn UI full table vẫn có stats đúng -> dùng ALL tables
            var orderStatsDict = await _orderStatsService
                .GetOrderStatsByTableIdsAsync(tables.Select(x => x.Id));

            // 10) Build result cho ALL tables
            var result = tables.Select(table =>
            {
                // active session theo map
                activeSessionByTable.TryGetValue(table.Id, out var activeSession);
                var sessionId = activeSession?.Id.ToString() ?? "";

                // counter chỉ tính nếu table có active session
                int pendingCount = 0;
                if (activeSession != null && pendingCountByTable.TryGetValue(table.Id, out var cnt))
                    pendingCount = cnt;

                // lastOrderUpdatedTime: chỉ lấy từ active order của session hiện tại
                // (đã query ở ordersInActiveSessions)
                DateTime? lastOrderUpdatedTime = null;

                if (activeSession != null)
                {
                    // Tìm order thuộc về activeSession này
                    var activeOrder = ordersInActiveSessions.FirstOrDefault(o => o.TableSessionId == activeSession.Id);
                    if (activeOrder != null)
                    {
                        lastOrderUpdatedTime = activeOrder.LastUpdatedTime;
                    }
                    else
                    {
                        // Hoặc fallback về thời điểm checkin nếu chưa có order? 
                        // Hoặc vẫn để null nếu muốn "chưa có order update"
                        // Tùy nhu cầu, ở đây để null hoặc checkin
                        // lastOrderUpdatedTime = activeSession.CheckIn; 
                    }
                }

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

                bool isWaitingDish =
                    pendingItems > 0 && table.Status == TableEnums.Occupied;

                int? waitingDurationInMinutes = null;
                if (isWaitingDish && lastOrderUpdatedTime.HasValue)
                {
                    var now = DateTime.UtcNow;
                    waitingDurationInMinutes =
                        (int)Math.Floor((now - lastOrderUpdatedTime.Value).TotalMinutes);
                }

                complaintsByTable.TryGetValue(table.Id, out var listComplain);

                // Nếu bàn trống (Available) -> reset hết info hiển thị
                if (table.Status == TableEnums.Available)
                {
                    lastOrderUpdatedTime = null;
                    waitingDurationInMinutes = null;
                    pendingItems = 0;
                    stats = new OrderStaticsResponse(); // reset stats visual
                    listComplain = null;
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
                    WaitingDurationInMinutes: waitingDurationInMinutes,
                    ListComplain: listComplain
                );
            }).ToDictionary(x => x.Id.ToString(), x => x);

            await _moderatorDashboardRefresher.PushSnapshotAsync();

            return new BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                result
            );
        }



     
       
        private async Task<(DateTime?, int, int, int, int, string)> GetOrderSnapshotAsync(Guid tableSessionId)
        {
            var order = await _unitOfWork.Repository<Order, Guid>()
                .GetWithSpecAsync(new BaseSpecification<Order>(o =>
                    o.TableSessionId == tableSessionId
                ));

            if (order == null)
                return (null, 0, 0,0, 0, null );

            var orderItems = await _unitOfWork.Repository<OrderItem, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<OrderItem>(i =>
                    i.OrderId == order.Id
                ));

            var kitchenCount = orderItems.Count(i =>
                i.Status == OrderItemStatus.Pending
                || i.Status == OrderItemStatus.Preparing
                || i.Status == OrderItemStatus.Remark
            );

            var waiterCount = orderItems.Count(i =>
                i.Status == OrderItemStatus.Ready
                || i.Status == OrderItemStatus.Served
                ||i.Status == OrderItemStatus.Completed
            );

            var cancelledCount = orderItems.Count(i =>
                i.Status == OrderItemStatus.Cancelled
            );
            var totalItemCount = orderItems.Count();

            return (
                order.LastUpdatedTime,
                kitchenCount,
                waiterCount,
                cancelledCount,
                totalItemCount ,
                order.Status.ToString()
            );
        }
        public async Task<BaseResponseModel<List<ComplainResponse>>> GetComplainByTable(
    Guid idTable,
    bool forCustomer = false
)
        {
            // 1️⃣ Lấy session active (luôn cần)
            var activeSession = await _unitOfWork.Repository<TableSession, Guid>()
                .GetWithSpecAsync(new BaseSpecification<TableSession>(s =>
                    s.TableId == idTable &&
                    s.CheckOut == null &&
                    s.Status == TableSessionStatus.Active
                ));

            // Không có session → không có complain hợp lệ
            if (activeSession == null)
            {
                return new BaseResponseModel<List<ComplainResponse>>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    new List<ComplainResponse>()
                );
            }

            // 2️⃣ Snapshot đơn hàng (dùng hàm bạn đã viết)
            var (lastOrderUpdatedTime, kitchenCount, waiterCount, cancelledCount, totalitemCount, orderStatus)
                = await GetOrderSnapshotAsync(activeSession.Id);

            // 3️⃣ Build spec complain (customer mới bị giới hạn theo session)
            var spec = new BaseSpecification<Complain>(c =>
                     c.TableId == idTable &&
                     c.CreatedTime >= activeSession.CheckIn &&
                     !activeSession.CheckOut.HasValue
                 );


            var complains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecWithInclueAsync(spec, true, c => c.Handler);

            if (complains == null || !complains.Any())
            {
                return new BaseResponseModel<List<ComplainResponse>>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    new List<ComplainResponse>()
                );
            }

            // 4️⃣ Map response theo ROLE
            var responseList = complains
                .OrderByDescending(c => c.CreatedTime)
                .Select(c =>
                {
                    var res = new ComplainResponse
                    {
                        ComplainId = c.Id,
                        IdTable = c.TableId,
                        FeedBack = c.Description,
                        CreateData = c.CreatedTime,
                        IsPending = c.isPending,
                        LastOrderUpdateTime = lastOrderUpdatedTime
                    };

                    if (!forCustomer)
                    {
                        // 👉 MODERATOR
                        res.KitchenItemCount = kitchenCount;
                        res.WaiterItemCount = waiterCount;
                        res.CancelledItemCount = cancelledCount;
                        res.ResolutionNote = c.ResolutionNote;
                        res.HandledBy = c.Handler?.FullName;
                        res.totalItemCount = totalitemCount;
                        res.OrderStatus = orderStatus;

                    }
                    else
                    {
                        // 👉 CUSTOMER
                        res.KitchenItemCount = 0;
                        res.WaiterItemCount = 0;
                        res.CancelledItemCount = 0;
                        res.HandledBy = null;
                        //res.ResolutionNote = BuildCustomerResolution(c.Title,c.ResolutionNote,c.isPending);
                        res.ResolutionNote = c.ResolutionNote;
                    }

                    return res;
                })
                .ToList();

            return new BaseResponseModel<List<ComplainResponse>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                responseList
            );
        }

        private string NormalizeTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "UNKNOWN";

            var t = title.Trim().ToLowerInvariant();

            if (t.Contains("phản hồi"))
                return "FEEDBACK";

            if (t.Contains("yêu cầu nhanh") || t.Contains("phục vụ nhanh"))
                return "QUICK_REQUEST";

            return "UNKNOWN";
        }

        private string BuildCustomerResolution(
     string? title,
     string? resolutionNote,
     bool isPending
 )
        {
            var normalizedTitle = NormalizeTitle(title);

            // ===============================
            // 1️⃣ ĐANG XỬ LÝ
            // ===============================
            if (isPending)
            {
                switch (normalizedTitle)
                {
                    case "FEEDBACK":
                        return "Phản hồi của bạn đã được ghi nhận. Nhân viên sẽ kiểm tra trong thời gian sớm nhất.";

                    case "QUICK_REQUEST":
                        return "Yêu cầu của bạn đã được chuyển đến nhân viên phục vụ.";

                    default:
                        return "Yêu cầu của bạn đang được xử lý.";
                }
            }

            // ===============================
            // 2️⃣ ĐÃ XỬ LÝ
            // ===============================
            switch (normalizedTitle)
            {
                case "FEEDBACK":
                    // Dù có resolutionNote hay không → KHÔNG show
                    return "Phản hồi của bạn đã được tiếp nhận và xử lý. Cảm ơn bạn đã thông báo.";

                case "QUICK_REQUEST":
                    // Có resolutionNote nội bộ → vẫn chỉ nói đã xử lý
                    return "Yêu cầu của bạn đã được xử lý.";

                default:
                    return "Yêu cầu của bạn đã được xử lý.";
            }
        }

        /// <summary>
        /// Parse resolutionNote và tạo QuickServeItem cho complain có Title = "Phục vụ nhanh"
        /// Ví dụ: "Phục vụ nhanh: Cho thêm nước mắm, cho thêm nước tương" 
        /// → Tạo 2 QuickServeItem: "Nước mắm" và "Nước tương"
        /// </summary>
        private async Task ProcessQuickServeItemsAsync(Guid complainId, string resolutionNote)
        {
            if (string.IsNullOrWhiteSpace(resolutionNote))
                return;

            // Xóa các QuickServeItem cũ của complain này (nếu có)
            var existingItems = await _unitOfWork.Repository<QuickServeItem, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<QuickServeItem>(q => q.ComplainId == complainId));
            
            if (existingItems != null && existingItems.Any())
            {
                foreach (var item in existingItems)
                {
                    await _unitOfWork.Repository<QuickServeItem, Guid>().DeleteAsync(item.Id);
                }
            }

            // Parse resolutionNote
            // Format: "Phục vụ nhanh: Cho thêm nước mắm, cho thêm nước tương"
            // Hoặc: "Phục vụ nhanh: Cho thêm nước mắm, cho thêm nước tương, cho thêm đũa"
            var items = _utilsService.ParseQuickServeItems(resolutionNote);

            // Tạo QuickServeItem mới
            var now = DateTime.UtcNow;
            foreach (var itemName in items)
            {
                var quickServeItem = new QuickServeItem
                {
                    Id = Guid.NewGuid(),
                    ComplainId = complainId,
                    ItemName = itemName.Trim(),
                    IsServed = false,
                    CreatedTime = now,
                    LastUpdatedTime = now
                };

                await _unitOfWork.Repository<QuickServeItem, Guid>().AddAsync(quickServeItem);
            }
        }

    }
}
