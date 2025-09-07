using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Notification
{
    /// <summary>
    /// DTO for order status change notifications
    /// </summary>
    public class OrderStatusNotification
    {
        public Guid OrderId { get; set; }
        public Guid TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public string NotificationType { get; set; } = "OrderStatusChanged";
    }
}
