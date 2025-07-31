using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Fillter
{
    public class ProductFillterResquest
    {
        public string? Search { get; set; }
        public string? CategoryName { get; set; }
        public string? ProductName { get; set; }
        public int? MinDuration { get; set; }
        public int? MaxDuration { get; set; }
        public string? Sort { get; set; }
    }
}
