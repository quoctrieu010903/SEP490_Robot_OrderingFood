using Microsoft.AspNetCore.SignalR;
using SEP490_Robot_FoodOrdering.API.Hubs;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Notification;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.API.Services
{
    /// <summary>
    /// Implementation of notification service using SignalR with the specific OrderNotificationHub
    /// This service is in the API layer to properly reference the OrderNotificationHub
    /// </summary>
    public class OrderNotificationService : INotificationService
    {
        private readonly IHubContext<OrderNotificationHub> _hubContext;
        private readonly ILogger<OrderNotificationService> _logger;

        public OrderNotificationService(IHubContext<OrderNotificationHub> hubContext, ILogger<OrderNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendOrderItemStatusNotificationAsync(OrderItemStatusNotification notification)
        {
            try
            {
                // Send to the specific table
                await _hubContext.Clients.Group($"Table_{notification.TableId}")
                    .SendAsync("OrderItemStatusChanged", notification);

                // Send to kitchen based on status
                if (ShouldNotifyKitchen(notification.NewStatus))
                {
                    await _hubContext.Clients.Group("Kitchen")
                        .SendAsync("OrderItemStatusChanged", notification);
                }

                // Send to waiters when item is ready to serve
                if (ShouldNotifyWaiters(notification.NewStatus))
                {
                    await _hubContext.Clients.Group("Waiters")
                        .SendAsync("OrderItemStatusChanged", notification);
                }

                // Always send to moderators for monitoring
                await _hubContext.Clients.Group("Moderators")
                    .SendAsync("OrderItemStatusChanged", notification);

                _logger.LogInformation(
                    "Order item status notification sent - OrderItem: {OrderItemId}, Status: {OldStatus} -> {NewStatus}",
                    notification.OrderItemId, notification.OldStatus, notification.NewStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order item status notification for OrderItem: {OrderItemId}", 
                    notification.OrderItemId);
            }
        }

        public async Task SendOrderStatusNotificationAsync(OrderStatusNotification notification)
        {
            try
            {
                // Send to the specific table
                await _hubContext.Clients.Group($"Table_{notification.TableId}")
                    .SendAsync("OrderStatusChanged", notification);

                // Send to all staff groups for awareness
                await _hubContext.Clients.Groups("Kitchen", "Waiters", "Moderators")
                    .SendAsync("OrderStatusChanged", notification);

                _logger.LogInformation(
                    "Order status notification sent - Order: {OrderId}, Status: {OldStatus} -> {NewStatus}",
                    notification.OrderId, notification.OldStatus, notification.NewStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order status notification for Order: {OrderId}", 
                    notification.OrderId);
            }
        }

        public async Task SendPaymentStatusNotificationAsync(PaymentStatusNotification notification)
        {
            try
            {
                // Send to the specific table
                await _hubContext.Clients.Group($"Table_{notification.TableId}")
                    .SendAsync("PaymentStatusChanged", notification);

                // Send to moderators for payment monitoring
                await _hubContext.Clients.Group("Moderators")
                    .SendAsync("PaymentStatusChanged", notification);

                _logger.LogInformation(
                    "Payment status notification sent - Order: {OrderId}, Status: {OldStatus} -> {NewStatus}",
                    notification.OrderId, notification.OldStatus, notification.NewStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment status notification for Order: {OrderId}", 
                    notification.OrderId);
            }
        }

        public async Task SendTableNotificationAsync(Guid tableId, string message, string notificationType)
        {
            try
            {
                var notification = new
                {
                    TableId = tableId,
                    Message = message,
                    NotificationType = notificationType,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"Table_{tableId}")
                    .SendAsync("TableNotification", notification);

                _logger.LogInformation("Table notification sent - Table: {TableId}, Type: {NotificationType}", 
                    tableId, notificationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send table notification to Table: {TableId}", tableId);
            }
        }

        public async Task SendKitchenNotificationAsync(string message, string notificationType)
        {
            try
            {
                var notification = new
                {
                    Message = message,
                    NotificationType = notificationType,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group("Kitchen")
                    .SendAsync("KitchenNotification", notification);

                _logger.LogInformation("Kitchen notification sent - Type: {NotificationType}", notificationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send kitchen notification");
            }
        }

        public async Task SendWaiterNotificationAsync(string message, string notificationType)
        {
            try
            {
                var notification = new
                {
                    Message = message,
                    NotificationType = notificationType,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group("Waiters")
                    .SendAsync("WaiterNotification", notification);

                _logger.LogInformation("Waiter notification sent - Type: {NotificationType}", notificationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send waiter notification");
            }
        }

        public async Task SendModeratorNotificationAsync(string message, string notificationType)
        {
            try
            {
                var notification = new
                {
                    Message = message,
                    NotificationType = notificationType,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group("Moderators")
                    .SendAsync("ModeratorNotification", notification);

                _logger.LogInformation("Moderator notification sent - Type: {NotificationType}", notificationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send moderator notification");
            }
        }

        /// <summary>
        /// Determines if kitchen staff should be notified based on order item status
        /// </summary>
        private static bool ShouldNotifyKitchen(OrderItemStatus status)
        {
            return status switch
            {
                OrderItemStatus.Pending => true,      // New order item to prepare
                OrderItemStatus.Preparing => true,    // Kitchen status update
                OrderItemStatus.Ready => true,        // Kitchen completed preparation
                OrderItemStatus.Remark => true,       // Item needs to be remade
                _ => false
            };
        }

        /// <summary>
        /// Determines if waiter staff should be notified based on order item status
        /// </summary>
        private static bool ShouldNotifyWaiters(OrderItemStatus status)
        {
            return status switch
            {
                OrderItemStatus.Ready => true,        // Ready to serve
                OrderItemStatus.Served => true,       // Confirmation of serving
                _ => false
            };
        }
    }
}
