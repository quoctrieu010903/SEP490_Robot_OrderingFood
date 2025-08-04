

using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
        public class OrdersByTableIdsSpecification : BaseSpecification<Order>
        {
            public OrdersByTableIdsSpecification(List<Guid>? tableIds)
            {

                var tableIdsList = tableIds.ToList(); // Convert to List để tối ưu performance

            if (tableIds != null && tableIds.Any())
            {
                AddCriteria(o => !o.DeletedTime.HasValue &&
                                o.TableId.HasValue &&
                                tableIds.Contains(o.TableId.Value));
            }
            else
            {
                // Handle case when tableIds is null or empty
                AddCriteria(o => false); // or whatever logic you need
            }
        }
       
    }
}
