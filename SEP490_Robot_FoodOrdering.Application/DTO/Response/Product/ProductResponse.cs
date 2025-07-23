using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Product
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int DurationTime { get; set; } // in seconds
        public decimal Price { get; set; } // Price of the product
        public string ImageUrl { get; set; } // URL of the image
        public string CategoryName { get; set; } // Name of the category

    }
}
