

using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class OrderbyModeratorCheckoutSpec : BaseSpecification<Order>
    {
        public OrderbyModeratorCheckoutSpec(Guid tableId) : base(o => !o.DeletedTime.HasValue && o.TableId == tableId &&
           o.TableSession != null && o.TableSession.Status == Enums.TableSessionStatus.Active &&
        o.TableSession.CheckOut == null)

        {

            AddOrderByDescending(o => o.CreatedTime); // Sắp xếp theo thời gian tạo mới nhất
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
                .Include(o => o.Payments));
        }

    }
}
