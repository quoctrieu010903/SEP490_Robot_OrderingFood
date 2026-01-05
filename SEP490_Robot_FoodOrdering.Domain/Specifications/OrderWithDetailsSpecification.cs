using SEP490_Robot_FoodOrdering.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications;

public class OrderWithDetailsSpecification : BaseSpecification<Order>
{
    public OrderWithDetailsSpecification(string token, string idTable)
      : base(order =>
          order.TableId.ToString() == idTable &&
          order.TableSession.Status == Enums.TableSessionStatus.Active &&
          order.TableSession.DeviceId == token
      )
    {
        AddIncludes();
    }

    private void AddIncludes()
    {
        ApplyInclude(q => q
            .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize) 
            .Include(o => o.OrderItems)
                     .ThenInclude(oi => oi.OrderItemTopping)
            .ThenInclude(oi => oi.Topping)
            .Include(o => o.Table)
                     .ThenInclude(o=> o.Sessions)
            .Include(o => o.Payments));
    } 
}