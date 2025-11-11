

using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
        public class OrdersByTableIdsSpecification : BaseSpecification<Order>
        {
        public OrdersByTableIdsSpecification(List<Guid>? tableIds ) : base(o => !o.DeletedTime.HasValue)
        {

            var tableIdsList = tableIds.ToList(); // Convert to List để tối ưu performance
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var todayVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).Date;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayVN, vnTimeZone);
            var tomorrow = startUtc.AddDays(1);

            if (tableIds != null && tableIds.Any())
            {
                        AddCriteria(o =>
                      !o.DeletedTime.HasValue &&
                      o.TableId.HasValue &&
                      tableIdsList.Contains(o.TableId.Value) &&
                      o.CreatedTime >= startUtc && o.CreatedTime < tomorrow
                  );
            }
            else
            {
                // Handle case when tableIds is null or empty
                AddCriteria(o => false); // or whatever logic you need
            }
            AddIncludes();
        }
        public OrdersByTableIdsSpecification(Guid TableID) : base(o => !o.DeletedTime.HasValue)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var todayVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).Date;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayVN, vnTimeZone);
            var tomorrow = startUtc.AddDays(1);
            AddCriteria(o =>
                !o.DeletedTime.HasValue &&
                o.TableId == TableID &&
                o.CreatedTime >= startUtc && o.CreatedTime < tomorrow
            );
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
