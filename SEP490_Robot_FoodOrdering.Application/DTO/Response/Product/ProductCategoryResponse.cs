using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Product
{
    public class ProductCategoryResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string UrlImg { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
