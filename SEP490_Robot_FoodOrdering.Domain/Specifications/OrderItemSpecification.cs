using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class OrderItemSpecification  : BaseSpecification<OrderItem>
    {
        public OrderItemSpecification() {
            AddIncludes();
        }
        private void AddIncludes()
        {
            ApplyInclude(x => x
                    .Include(o => o.Order)
                         .ThenInclude(o => o.Table)); // nếu Order có Table
            // nếu Order có Table



        }
    }
}
