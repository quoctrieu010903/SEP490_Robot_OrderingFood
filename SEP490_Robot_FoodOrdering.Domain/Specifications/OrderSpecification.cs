
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class OrderSpecification : BaseSpecification<Order>
    {

        public OrderSpecification(Guid tableId) : base(o => !o.DeletedTime.HasValue && o.TableId == tableId && o.Status == OrderStatus.Pending)
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
        (string.IsNullOrEmpty(productName) ||
         o.OrderItems.Any(oi =>
            oi.Product != null &&
            !string.IsNullOrEmpty(oi.Product.Name) &&
            oi.Product.Name.ToLower().Contains(productName.ToLower()))
        ))
        {
            AddIncludes();
        }





        // Lấy theo orderId
        public OrderSpecification(Guid orderId, bool byOrderId)
        : base(o => o.Id == orderId && !o.DeletedTime.HasValue)
        {   
            AddIncludes();
        }

    public OrderSpecification(Guid orderId, Guid tableId, bool byOrderId) :base(o=> o.Id == orderId && o.TableId == tableId && !o.DeletedTime.HasValue  ) {
            AddIncludes();
        }

    // Get all orders by table ID for payment (not filtered by status)
    public OrderSpecification(bool forPayment, Guid tableId) : base(o => !o.DeletedTime.HasValue && o.TableId == tableId)
    {
        AddIncludes();
    }

    // Get orders by table ID with Delivering status for payment
    public OrderSpecification(Guid tableId, OrderStatus status) : base(o => !o.DeletedTime.HasValue && o.TableId == tableId && o.Status == status)
    {
        AddIncludes();
    }
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
                .Include(o => o.Payment));
        }   
    }

}
