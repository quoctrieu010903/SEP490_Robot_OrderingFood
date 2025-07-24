using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class CreateToppingRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public IFormFile ImageUrl { get; set; }
    }
}
