
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class OrderSpecification : BaseSpecification<Order>
    {

        public OrderSpecification(Guid tableId)
            // old logic (incorrect precedence) kept for reference:
            // : base(o => !o.DeletedTime.HasValue && o.TableId == tableId && o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed)
            : base(o =>
                !o.DeletedTime.HasValue &&
                o.TableId == tableId &&
                (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed))
        {
            AddIncludes();
        }
      
        public OrderSpecification()
        : base(o => !o.DeletedTime.HasValue &&  o.CreatedTime.Date == DateTime.UtcNow.Date)
        {

            AddIncludes();
        }
       
        public OrderSpecification(string? productName, DateTime startUtc, DateTime endUtc)
     : base(o =>
         !o.DeletedTime.HasValue &&
        o.CreatedTime >= startUtc &&
        o.CreatedTime < endUtc &&
        o.TableSession != null &&
        o.TableSession.Status == TableSessionStatus.Active &&
        o.TableSession.CheckOut == null &&
        (string.IsNullOrEmpty(productName) ||
         o.OrderItems.Any(oi =>
            oi.Product != null &&
            !string.IsNullOrEmpty(oi.Product.Name) &&
            oi.Product.Name.ToLower().Contains(productName.ToLower()))
        ))
        {
            // Ưu tiên các order có món remake, sắp xếp theo thời gian remake mới nhất
            AddOrderByDescending(o =>
                o.OrderItems
                    .Where(oi => oi.RemakeOrderItems != null && oi.RemakeOrderItems.Any())
                    .SelectMany(oi => oi.RemakeOrderItems)
                    .Max(r => (DateTime?)r.LastUpdatedTime) ?? o.CreatedTime);

            AddOrderByDescending(o => o.CreatedTime);

            AddIncludes();
        }





        // Lấy theo orderId
        public OrderSpecification(Guid orderId, bool byOrderId)
        : base(o => o.Id == orderId && !o.DeletedTime.HasValue)
        {   
            AddIncludes();
        }
        public OrderSpecification(Guid tableId, Guid sessionId)
        : base(o =>
            o.TableId == tableId &&
            o.TableSessionId == sessionId &&
            !o.DeletedTime.HasValue &&
            o.Status != OrderStatus.Completed &&
            o.Status != OrderStatus.Cancelled)
        {
            AddIncludes();
            AddOrderByDescending(o => o.CreatedTime);
        }
        public OrderSpecification(Guid orderId, Guid tableId, bool byOrderId) :base(o=> o.Id == orderId && o.TableId == tableId && !o.DeletedTime.HasValue  ) {
            AddIncludes();
        }

    // Get all orders by table ID for payment (not filtered by status)
    public OrderSpecification( Guid tableId, DateTime? startDate , DateTime? endDate ) : base(o => !o.DeletedTime.HasValue && o.TableId == tableId &&
        (!startDate.HasValue || o.CreatedTime >= startDate.Value) &&
        (!endDate.HasValue || o.CreatedTime < endDate.Value))
    {
           
            AddOrderByDescending(o => o.CreatedTime); // Sắp xếp theo thời gian tạo mới nhất
            AddIncludes();
    }

        // Get orders by table ID with Delivering status for payment
        public OrderSpecification(Guid tableId, OrderStatus status, bool onlyActiveSession)
            : base(o =>
                !o.DeletedTime.HasValue
                && o.TableId == tableId
                && o.Status == status
                && (!onlyActiveSession
                    || (o.TableSession != null
                        && o.TableSession.Status == TableSessionStatus.Active))
            )
        {
            AddIncludes();
        }
        // Get orders by table IDs for the current day


        private void AddIncludes()
        {
            ApplyInclude(q => q
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize) // Ensure ProductSize is included
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.OrderItemTopping)
                        .ThenInclude(oi => oi.Topping)
                .Include(o => o.Table)
                .Include(o => o.Payments));
        }   
    }

}
