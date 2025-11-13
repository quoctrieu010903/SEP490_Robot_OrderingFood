using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    /// <summary>
    /// Specification to get invoices by OrderId
    /// </summary>
    public class InvoiceByOrderIdSpecification : BaseSpecification<Invoice>
    {
        public InvoiceByOrderIdSpecification(Guid orderId)
            : base(x => x.OrderId == orderId && !x.DeletedTime.HasValue)
        {
            ApplyInclude(i => i.Include(x => x.Table)
                                .Include(x => x.Order)
                                .Include(x => x.Details)
                                    .ThenInclude(d => d.OrderItem));
        }
    }
}

