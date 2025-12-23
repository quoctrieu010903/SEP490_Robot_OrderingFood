using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain;

public class PendingComplainsInActiveSessionSpec : BaseSpecification<Complain>
{
    public PendingComplainsInActiveSessionSpec()
        : base(c =>
            c.isPending
           
        )
    {
     
    }

}
