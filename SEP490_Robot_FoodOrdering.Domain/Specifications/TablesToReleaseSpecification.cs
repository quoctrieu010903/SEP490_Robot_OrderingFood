

using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class TablesToReleaseSpecification : BaseSpecification<Table>
    {
        public TablesToReleaseSpecification(double autoReleaseMinutes)
            : base(t =>
                t.Status == TableEnums.Occupied &&
                t.LockedAt.HasValue &&
                (DateTime.UtcNow - t.LockedAt.Value).TotalMinutes >= autoReleaseMinutes
            )
        {
            ApplyInclude(t => t.Include(o => o.Orders));
        }
    }
    
    }

