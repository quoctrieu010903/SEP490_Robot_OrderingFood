using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class InvoiceSpecification : BaseSpecification<Invoice>
    {
        // Constructor cho paging
        //public InvoiceSpecification(int pageIndex, int pageSize)
        //    : base(pageIndex, pageSize)
        //{
        //    AddInvoiceIncludes();
        //}

        // Constructor cho Id
        public InvoiceSpecification(Guid invoiceId)
            : base(x => x.Id == invoiceId)
        {
            AddInvoiceIncludes();
        }

        // Constructor cho TableId + paging
        public InvoiceSpecification(Guid tableId, int pageIndex, int pageSize)
                : base(x => x.Table.Id == tableId)
        {
            AddInvoiceIncludes();
        }


        /// <summary>
        /// Gom tất cả các include của Invoice vào 1 hàm riêng
        /// </summary>
        private void AddInvoiceIncludes()
        {
            ApplyInclude(i => i.Include(x => x.Table)
                                .Include(x => x.Details)
                                     .ThenInclude(x => x.OrderItem)
                                          .ThenInclude(oi => oi.Order)

                                .Include(x => x.Details)
                                        .ThenInclude(d => d.OrderItem)
                                            .ThenInclude(oi => oi.Product)
                                                .ThenInclude(p => p.AvailableToppings)
                           .Include(x => x.Details)
                                        .ThenInclude(d => d.OrderItem)
                                            .ThenInclude(oi => oi.Product)
                                                .ThenInclude(p => p.Sizes));














        }
    }

}
