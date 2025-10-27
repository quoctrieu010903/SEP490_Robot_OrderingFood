
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class ComplainSpecification : BaseSpecification<Complain>
    {
        public ComplainSpecification(bool isPendingOnly = true)
            : base(x => x.isPending) // nếu truyền true → lọc pending
        {
            AddOrderByDescending(x => x.CreatedTime);
            
        }
    }

}
