using System;
using System.Collections.Generic;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class CreateOrderRequest
    {
        public Guid TableId { get; set; }
        public string? deviceToken {  get; set; }
        public List<CreateOrderItemRequest> Items { get; set; }
    }

    public class CreateOrderItemRequest
    {
        public string? Note { get; set; }
        public Guid ProductId { get; set; }
        public Guid ProductSizeId { get; set; }
        public List<Guid> ToppingIds { get; set; }
    }
} 