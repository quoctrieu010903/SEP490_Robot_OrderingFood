using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain;

public class FirstOrderInSessionsSpec : BaseSpecification<Order>
{
    public FirstOrderInSessionsSpec(HashSet<Guid> sessionIds)
        : base(o => o.TableSessionId != null && sessionIds.Contains(o.TableSessionId.Value))
    {
        
        AddOrderBy(o => o.CreatedTime);
    }
}
