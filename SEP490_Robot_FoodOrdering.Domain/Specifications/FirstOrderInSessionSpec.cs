

using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class FirstOrderInSessionSpec : BaseSpecification<Order>
    {
        public FirstOrderInSessionSpec(Guid tableId, Guid sessionId)
            : base(o =>
                !o.DeletedTime.HasValue
                && o.TableId == tableId
                && o.TableSessionId == sessionId
            )
        {
            // lấy order sớm nhất trong session
            AddOrderBy(o => o.CreatedTime);
            ApplyPaging(0, 1); // Take(1)
        }
    }
    
}
