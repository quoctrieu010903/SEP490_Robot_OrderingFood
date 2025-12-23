

using SEP490_Robot_FoodOrdering.Application.DTO.Response;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class OrderStatsQuery : IOrderStatsQuery
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderStatsQuery(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
       

        private static OrderStaticsResponse CalculateOrderStats(IEnumerable<Order> orders)
        {
            // 🧩 Bàn chưa có order nào
            if (orders == null || !orders.Any())
            {
                return new OrderStaticsResponse
                {
                    PaymentStatus = PaymentStatusEnums.None,
                    TotalOrderItems = 0,
                    DeliveredCount = 0,
                    ServedCount = 0,
                    PaidCount = 0
                };
            }

            // 🔹 Gom tất cả item của các order
            var allItems = orders
                .Where(o => o.OrderItems != null)
                .SelectMany(o => o.OrderItems.Select(item => new
                {
                    OrderPaymentStatus = o.PaymentStatus,
                    OrderStatus = o.Status,
                    ItemStatus = item.Status,
                    PaidCount = item.PaymentStatus == PaymentStatusEnums.Paid
                }))
                .ToList();

            var totalItems = allItems.Count;

            // 🔹 Đếm số món đã thanh toán (Completed + Order đã Paid)
            var paidItems = allItems.Count(x =>
                x.ItemStatus == OrderItemStatus.Completed &&
                (x.OrderPaymentStatus == PaymentStatusEnums.Paid || x.OrderPaymentStatus == PaymentStatusEnums.Refunded));

            // 🔹 Xác định trạng thái tổng hợp của các order
            bool allCancelledOrders = orders.All(o => o.Status == OrderStatus.Cancelled);
            bool allCompletedOrders = orders.All(o => o.Status == OrderStatus.Completed);
            bool hasActiveOrder = orders.Any(o =>
                o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

            PaymentStatusEnums finalPaymentStatus;

            // ✅ 1️⃣ Nếu toàn bộ order bị huỷ → bàn không còn thanh toán nào
            if (allCancelledOrders)
            {
                finalPaymentStatus = PaymentStatusEnums.None;
            }
            // ✅ 2️⃣ Nếu toàn bộ order đã hoàn tất → đã thanh toán
            else if (allCompletedOrders)
            {
                finalPaymentStatus = PaymentStatusEnums.Paid;
            }
            // ✅ 3️⃣ Nếu bàn còn order đang hoạt động
            else if (hasActiveOrder)
            {
                var currentOrder = orders
                    .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                    .OrderByDescending(o => o.CreatedTime) // nếu có thuộc tính CreatedOn
                    .FirstOrDefault();

                // Nếu không có CreatedOn thì dùng ID lớn nhất (hoặc logic khác)
                finalPaymentStatus = currentOrder?.PaymentStatus ?? PaymentStatusEnums.None;
            }
            // ✅ 4️⃣ Nếu không còn order hoạt động (tức tất cả done hoặc cancel)
            else
            {
                finalPaymentStatus = PaymentStatusEnums.None;
            }

            // 🔹 Trả về kết quả thống kê
            return new OrderStaticsResponse
            {
                PaymentStatus = finalPaymentStatus,
                TotalOrderItems = totalItems,
                DeliveredCount = allItems.Count(x =>
                    x.ItemStatus is OrderItemStatus.Preparing or OrderItemStatus.Ready or OrderItemStatus.Served or OrderItemStatus.Remark or OrderItemStatus.Completed),
                ServedCount = allItems.Count(x =>
                    x.ItemStatus is OrderItemStatus.Served or OrderItemStatus.Completed),
                PaidCount = paidItems
            };
        }

        public async Task<Dictionary<Guid, OrderStaticsResponse>> GetOrderStatsByTableIdsAsync(IEnumerable<Guid> tableIds)
        {
            var tableIdsList = tableIds.ToList();
            if (!tableIdsList.Any())
                return new Dictionary<Guid, OrderStaticsResponse>();

            // 🔹 Lấy tất cả orders của các bàn trong 1 query duy nhất
            var allOrders = await _unitOfWork.Repository<Order, Guid>()
                .GetAllWithSpecAsync(new OrdersByTableIdsSpecification(tableIdsList), true);

            // 🔹 Gom nhóm order theo TableId
            var ordersByTableId = allOrders
                .GroupBy(o => o.TableId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new Dictionary<Guid, OrderStaticsResponse>(tableIdsList.Count);

            // 🔹 Xử lý từng bàn
            foreach (var tableId in tableIdsList)
            {
                var orders = ordersByTableId.GetValueOrDefault(tableId, new List<Order>());
                result[tableId] = CalculateOrderStats(orders);
            }

            return result;
        }
    
    }
}
