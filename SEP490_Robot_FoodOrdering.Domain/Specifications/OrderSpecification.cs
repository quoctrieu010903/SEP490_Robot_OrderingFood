
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
        : base(o => !o.DeletedTime.HasValue)
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
        private void AddIncludes()
        {
            ApplyInclude(q => q
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(oi => oi.Sizes)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.OrderItemTopping)
                        .ThenInclude(oi => oi.Topping)
                    
                        

                .Include(o => o.Table)
                .Include(o => o.Payment));
        }
    }

}
