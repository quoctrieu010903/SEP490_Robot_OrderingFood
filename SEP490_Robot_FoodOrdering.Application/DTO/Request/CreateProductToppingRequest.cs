using System;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class CreateProductToppingRequest
    {
        public Guid ProductId { get; set; }
        public Guid ToppingId { get; set; }
    }
} 