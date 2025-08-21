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
        public TableSpecification(int pageIndex, int pageSize, TableEnums? status, string? TableName)
    :   base(t => !t.DeletedTime.HasValue && (!status.HasValue || t.Status == status.Value) &&
             (string.IsNullOrEmpty(TableName) || t.Name.ToLower().Contains(TableName.ToLower())))
        {
            PageIndex = pageIndex;
            PageSize = pageSize;

            ApplyInclude(q => q.Include(q => q.Orders));
            AddOrderBy(t => t.Name);
            
        }

    }
}
