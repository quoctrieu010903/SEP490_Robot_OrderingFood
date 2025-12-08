using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class TableByIdWithOrdersSessionsSpecification : BaseSpecification<Table>
    {
        public TableByIdWithOrdersSessionsSpecification(Guid tableId)
            : base(t => t.Id == tableId)
        {
            ApplyInclude(q => q
                .Include(t => t.Orders)
                .Include(t => t.Sessions)
            );
        }
    }
}
