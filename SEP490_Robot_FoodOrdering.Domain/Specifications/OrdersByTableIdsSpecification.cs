using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class OrdersByTableIdsSpecification : BaseSpecification<Order>
    {
        // ✅ CONSTRUCTOR: LẤY ORDER THEO SESSION ACTIVE
        public OrdersByTableIdsSpecification(List<Guid> tableIds)
            : base(o =>
                !o.DeletedTime.HasValue &&
                o.TableSession != null &&
                o.TableSession.Status == TableSessionStatus.Active &&
                o.TableSession.CheckOut == null &&
                tableIds.Contains(o.TableSession.TableId)
            )
        {
            AddIncludes();
        }

        // 📅 CONSTRUCTOR: LẤY ORDER THEO TABLE + NGÀY (GIỮ NGUYÊN)
        public OrdersByTableIdsSpecification(Guid tableId)
            : base(o => !o.DeletedTime.HasValue)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var todayVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).Date;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayVN, vnTimeZone);
            var tomorrow = startUtc.AddDays(1);

            AddCriteria(o =>
                !o.DeletedTime.HasValue &&
                o.TableId == tableId &&
                o.CreatedTime >= startUtc &&
                o.CreatedTime < tomorrow
            );

            AddIncludes();
        }

        private void AddIncludes()
        {
            ApplyInclude(q => q
                .Include(o => o.Customer)
                .Include(o => o.TableSession)
                    .ThenInclude(ts => ts.Activities)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.OrderItemTopping)
                        .ThenInclude(t => t.Topping)
                .Include(o => o.Table)
                .Include(o => o.Payments)
            );
        }
    }
}
