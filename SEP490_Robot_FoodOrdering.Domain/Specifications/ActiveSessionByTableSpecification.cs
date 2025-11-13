using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    /// <summary>
    /// Specification to get active session by table ID
    /// </summary>
    public class ActiveSessionByTableSpecification : BaseSpecification<TableSession>
    {
        public ActiveSessionByTableSpecification(Guid tableId)
            : base(x => x.TableId == tableId 
                        && x.Status == TableSessionStatus.Active 
                        && !x.DeletedTime.HasValue)
        {
            ApplyInclude(s => s.Include(x => x.Table)
                                .Include(x => x.Orders)
                                    .ThenInclude(o => o.OrderItems));
        }
    }
}

