using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Notification
{
    /// <summary>
    /// DTO for payment status change notifications
    /// </summary>
    public class PaymentStatusNotification
    {
        public Guid OrderId { get; set; }
        public Guid TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public PaymentStatusEnums OldStatus { get; set; }
        public PaymentStatusEnums NewStatus { get; set; }
        public PaymentMethodEnums PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string NotificationType { get; set; } = "PaymentStatusChanged";
    }
}
