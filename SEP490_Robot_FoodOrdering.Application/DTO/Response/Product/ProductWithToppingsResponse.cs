using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Product
{
    public class ProductWithToppingsResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }

        public List<ToppingResponse> Toppings { get; set; }
    }

}
