using System;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class UpdateOrderItemStatusRequest
    {
        public OrderItemStatus Status { get; set; }
        public string? Note { get; set; } // Optional note for the status update

    }
} 