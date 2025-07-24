using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications.Params
{
    public class BaseSpecParams
    {
        public int? PageIndex { get; set; } = 1;
        public int? PageSize { get; set; }
        public string? Search { get; set; }
        public string? Sort { get; set; }
    }

    public class AdvancedFilter
    {
        public string FieldName { get; set; } = null!;
        public FilterOperator? Operator { get; set; }
        public string? Value { get; set; }
    }
}

