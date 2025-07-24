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
        public int DurationTime { get; set; } // in seconds
        public string ImageUrl { get; set; } // URL of the image

    }
}
