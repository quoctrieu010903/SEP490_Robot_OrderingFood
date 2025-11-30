

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
       (DateTime.UtcNow - t.LockedAt.Value).TotalMinutes >= autoReleaseMinutes &&

       // ✅ thêm điều kiện: session mới nhất (Active) không có order
       !t.Orders.Any(o =>
           !o.DeletedTime.HasValue
           && o.TableSessionId.HasValue
           && t.Sessions.Any(ts =>
               ts.Id == o.TableSessionId.Value
               && ts.Status == TableSessionStatus.Active
           )
       )
   )
        {
            ApplyInclude(q => q.Include(x => x.Orders));
            ApplyInclude(q => q.Include(x => x.Sessions)); // ✅ cần có navigation này
            ApplyInclude(q => q.Include(x => x.Complains));
        }

    }
}
    
    

