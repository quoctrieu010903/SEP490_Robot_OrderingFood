using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class TableSpecification : BaseSpecification<Table>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public TableSpecification(int pageIndex, int pageSize)
            : base(t => !t.DeletedTime.HasValue &&  t.Status==TableEnums.Available ) // No specific filter, just pagination
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            // Include related entities if necessary
            ApplyInclude(q => q
                .Include(q => q.Orders))    ;
        }
    }
}
