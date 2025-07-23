using System;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Product
{
    public class ProductToppingResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid ToppingId { get; set; }
    }
} 