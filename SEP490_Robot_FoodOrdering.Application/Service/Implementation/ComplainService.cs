

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

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ComplainService : IComplainService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOrderStatsQuery _orderStatsService;
        private readonly IModeratorDashboardRefresher _moderatorDashboardRefresher;
        public ComplainService(IUnitOfWork unitOfWork, IMapper mapper, IOrderStatsQuery orderStatsService , IModeratorDashboardRefresher moderatorDashboardRefresher)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderStatsService = orderStatsService;
            _moderatorDashboardRefresher = moderatorDashboardRefresher;

        }

        public async Task<BaseResponseModel<List<ComplainCreate>>> ComfirmComplain(
        Guid idTable,
        List<Guid>? IDFeedback,
        bool isPending,
        string content)
        {
            // 🔹 1️⃣ Lấy tất cả complain theo bàn
            var feedbackEntities = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecWithInclueAsync(
                    new BaseSpecification<Complain>(f => f.TableId == idTable),
                    true,
                    f => f.OrderItem, // include nếu có, vẫn null-safe
                    f => f.OrderItem.Product
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

                await _unitOfWork.Repository<Complain, Guid>().UpdateAsync(feedback);

                // 🧩 Mapping ra DTO an toàn
                updatedFeedbacks.Add(new ComplainCreate(
                    feedback.CreatedTime,
                    feedback.isPending,
                    feedback.Description +
                    (feedback.OrderItem != null ? $" (Món: {feedback.OrderItem.Product?.Name})" : "")
                ));
            }

            // 🔹 4️⃣ Lưu thay đổi
            await _unitOfWork.SaveChangesAsync();

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

            
            if (request.OrderItemIds == null || !request.OrderItemIds.Any())
            {
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
            }
            else
            {
                // 🔹 Case 2: Khiếu nại theo từng OrderItem cụ thể
                foreach (var orderItemId in request.OrderItemIds)
                {
                    var existedItem = await _unitOfWork.Repository<OrderItem, Guid>().GetByIdAsync(orderItemId);
                    if (existedItem == null)
                        throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Không tìm thấy OrderItem: {orderItemId}");

                    var complain = new Complain
                    {
                        Id = Guid.NewGuid(),
                        TableId = request.TableId,
                        OrderItemId = orderItemId,
                        Title = request.Title,
                        Description = request.ComplainNote,
                        isPending = true, // ❗ pending để waiter/bếp xử lý
                        CreatedTime = DateTime.UtcNow,
                        LastUpdatedTime = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<Complain, Guid>().AddAsync(complain);
                }
            }

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
            var tables = await _unitOfWork.Repository<Table, Guid>()
                .GetAllWithIncludeAsync(true, t => t.Orders, t => t.Sessions);

            var complains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<Complain>(x => x.isPending));

            if (tables == null || !tables.Any())
                throw new ErrorException(404, "No tables found");

            var orderStatsDict = await _orderStatsService.GetOrderStatsByTableIdsAsync(tables.Select(x => x.Id));
               

            var result = tables.Select(table =>
            {
                int pendingCount = complains.Count(c => c.TableId == table.Id);

                var activeSession = table.Sessions
                    .Where(s => s.Status == TableSessionStatus.Active)
                    .OrderByDescending(s => s.CheckIn)
                    .FirstOrDefault();

                var sessionId = activeSession?.Id.ToString() ?? string.Empty;

                DateTime? lastOrderUpdatedTime = table.Orders != null && table.Orders.Any()
                    ? table.Orders
                        .OrderByDescending(o => o.LastUpdatedTime)
                        .Select(o => (DateTime?)o.LastUpdatedTime)
                        .FirstOrDefault()
                    : null;

                // mặc định stats = 0
                var stats = new OrderStaticsResponse
                {
                    PaymentStatus = 0,
                    DeliveredCount = 0,
                    ServedCount = 0,
                    PaidCount = 0,
                    TotalOrderItems = 0
                };

                // Nếu có session active và có thống kê thì lấy
                if (activeSession != null && orderStatsDict.TryGetValue(table.Id, out var s))
                {
                    stats = s;
                }

                // Nếu bàn trống + không có session active → ép về 0 luôn cho chắc
                if (table.Status == (int)TableEnums.Available && activeSession == null)
                {
                    stats = new OrderStaticsResponse
                    {
                        PaymentStatus = 0,
                        DeliveredCount = 0,
                        ServedCount = 0,
                        PaidCount = 0,
                        TotalOrderItems = 0
                    };
                    lastOrderUpdatedTime = null;
                }

                // 👉 Số món chưa serve (Completed coi như đã serve)
                var pendingItems = Math.Max(0, stats.TotalOrderItems - stats.ServedCount);

                // Bàn đang chờ món nếu:
                // - còn món chưa serve
                // - bàn đang có khách
                bool isWaitingDish =
                    pendingItems > 0 && table.Status == TableEnums.Occupied;

                int? waitingDurationInMinutes = null;
                if (isWaitingDish && lastOrderUpdatedTime.HasValue)
                {
                    var now = DateTime.UtcNow; // hoặc DateTime.Now tùy convention
                    waitingDurationInMinutes =
                        (int)Math.Floor((now - lastOrderUpdatedTime.Value).TotalMinutes);
                }

                // TODO: nếu muốn FE hiển thị pill "Chờ món: X phút"
                // thì thêm pendingItems / isWaitingDish / waitingDurationInMinutes
                // vào ComplainPeedingInfo

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
            // 1) Nếu customer -> lấy Active Session mới nhất của bàn
            Guid? activeSessionId = null;

            if (forCustomer)
            {
                var activeSession = await _unitOfWork.Repository<TableSession, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<TableSession>(s =>
                        s.TableId == idTable
                        && s.Status == TableSessionStatus.Active
                    ));

                if (activeSession == null)
                    throw new ErrorException(
                        StatusCodes.Status404NotFound,
                        ResponseCodeConstants.NOT_FOUND,
                        "Bàn hiện không có phiên hoạt động (Active session)."
                    );

                activeSessionId = activeSession.Id;
            }

            // 2) Build predicate
            // Customer: lọc theo TableId + ActiveSessionId
            // Moderator/Admin: lọc theo TableId (lấy tất cả)
            var spec = new BaseSpecification<Complain>(c =>
        c.TableId == idTable &&
        (!forCustomer || c.Table.Sessions.Any(s => s.Id == activeSessionId))
    );


            // 3) Query + include OrderItem + Product
            var complains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecWithInclueAsync(
                    spec,
                    true,
                    o => o.OrderItem,
                    o => o.OrderItem.Product
                );

            if (complains == null || !complains.Any())
                throw new ErrorException(
                    StatusCodes.Status404NotFound,
                    ResponseCodeConstants.NOT_FOUND,
                    "Không tìm thấy complain"
                );

            // 4) Map response
            var responseList = complains.Select(c => new ComplainResponse
            {
                ComplainId = c.Id,
                IdTable = c.TableId,
                FeedBack = c.Description,
                CreateData = c.CreatedTime,
                IsPending = c.isPending,
                ResolutionNote = c.ResolutionNote,

                Dtos = c.OrderItem != null
                    ? new List<OrderItemDTO>
                    {
                new OrderItemDTO(
                    c.OrderItem.Id,
                    c.OrderItem.Product?.Name ?? "N/A",
                    c.OrderItem.Product?.ImageUrl ?? "N/A",
                    c.OrderItem.Status
                )
                    }
                    : new List<OrderItemDTO>()
            }).ToList();

            return new BaseResponseModel<List<ComplainResponse>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                responseList
            );
        }


    }
}
