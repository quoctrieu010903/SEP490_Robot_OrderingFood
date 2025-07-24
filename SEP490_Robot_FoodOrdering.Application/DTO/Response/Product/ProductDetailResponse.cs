using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Product
{
    public class ProductDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UrlImg { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public List<ProductSizeResponse> Sizes { get; set; }
    }
   
}
