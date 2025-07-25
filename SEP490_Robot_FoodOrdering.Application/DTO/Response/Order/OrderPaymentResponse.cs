using System;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Order
{
    public class OrderPaymentResponse
    {
        public Guid OrderId { get; set; }
        public PaymentStatusEnums PaymentStatus { get; set; }
        public string? PaymentUrl { get; set; } // For online payment (VNPay)
        public string? Message { get; set; }
    }
} 