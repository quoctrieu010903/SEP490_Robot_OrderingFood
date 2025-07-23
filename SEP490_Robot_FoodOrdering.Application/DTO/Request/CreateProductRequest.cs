using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class CreateProductRequest
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int DurationTime { get; set; } // in seconds
        public IFormFile ImageUrl { get; set; }
    }
}
