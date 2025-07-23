using System;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Product
{
    public class ProductSizeResponse
    {
        public Guid Id { get; set; }
        public SizeNameEnum SizeName { get; set; }
        public decimal Price { get; set; }
        public Guid ProductId { get; set; }
    }
} 