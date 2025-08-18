using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Notification
{
    /// <summary>
    /// DTO for order item status change notifications
    /// </summary>
    public class OrderItemStatusNotification
    {
        public Guid OrderId { get; set; }
        public Guid OrderItemId { get; set; }
        public Guid TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SizeName { get; set; } = string.Empty;
        public OrderItemStatus OldStatus { get; set; }
        public OrderItemStatus NewStatus { get; set; }
        public string? RemarkNote { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public string NotificationType { get; set; } = "OrderItemStatusChanged";
    }
}
