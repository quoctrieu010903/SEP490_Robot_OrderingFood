using SEP490_Robot_FoodOrdering.Application.DTO.Response.Notification;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    /// <summary>
    /// Service for sending real-time notifications via SignalR
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends order item status change notification to relevant clients
        /// </summary>
        /// <param name="notification">Order item status notification data</param>
        Task SendOrderItemStatusNotificationAsync(OrderItemStatusNotification notification);

        /// <summary>
        /// Sends order status change notification to relevant clients
        /// </summary>
        /// <param name="notification">Order status notification data</param>
        Task SendOrderStatusNotificationAsync(OrderStatusNotification notification);

        /// <summary>
        /// Sends payment status change notification to relevant clients
        /// </summary>
        /// <param name="notification">Payment status notification data</param>
        Task SendPaymentStatusNotificationAsync(PaymentStatusNotification notification);

        /// <summary>
        /// Sends a general notification to a specific table
        /// </summary>
        /// <param name="tableId">The table ID</param>
        /// <param name="message">The message to send</param>
        /// <param name="notificationType">The type of notification</param>
        Task SendTableNotificationAsync(Guid tableId, string message, string notificationType);

        /// <summary>
        /// Sends a notification to all kitchen staff
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="notificationType">The type of notification</param>
        Task SendKitchenNotificationAsync(string message, string notificationType);

        /// <summary>
        /// Sends a notification to all waiter staff
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="notificationType">The type of notification</param>
        Task SendWaiterNotificationAsync(string message, string notificationType);

        /// <summary>
        /// Sends a notification to all moderators
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="notificationType">The type of notification</param>
        Task SendModeratorNotificationAsync(string message, string notificationType);
    }
}
