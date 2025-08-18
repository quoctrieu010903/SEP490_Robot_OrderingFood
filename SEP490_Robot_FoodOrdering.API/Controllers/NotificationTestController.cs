using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Notification;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    /// <summary>
    /// Test controller for SignalR notifications (for development/testing only)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationTestController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationTestController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Test order item status notification
        /// </summary>
        [HttpPost("test-order-item-status")]
        public async Task<IActionResult> TestOrderItemStatusNotification()
        {
            var notification = new OrderItemStatusNotification
            {
                OrderId = Guid.NewGuid(),
                OrderItemId = Guid.NewGuid(),
                TableId = Guid.NewGuid(),
                TableName = "Table 1",
                ProductName = "Test Product",
                SizeName = "Medium",
                OldStatus = OrderItemStatus.Pending,
                NewStatus = OrderItemStatus.Preparing,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "Test User"
            };

            await _notificationService.SendOrderItemStatusNotificationAsync(notification);
            return Ok(new { message = "Order item status notification sent", notification });
        }

        /// <summary>
        /// Test order status notification
        /// </summary>
        [HttpPost("test-order-status")]
        public async Task<IActionResult> TestOrderStatusNotification()
        {
            var notification = new OrderStatusNotification
            {
                OrderId = Guid.NewGuid(),
                TableId = Guid.NewGuid(),
                TableName = "Table 2",
                OldStatus = OrderStatus.Pending,
                NewStatus = OrderStatus.Preparing,
                TotalPrice = 150000,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "Test User"
            };

            await _notificationService.SendOrderStatusNotificationAsync(notification);
            return Ok(new { message = "Order status notification sent", notification });
        }

        /// <summary>
        /// Test payment status notification
        /// </summary>
        [HttpPost("test-payment-status")]
        public async Task<IActionResult> TestPaymentStatusNotification()
        {
            var notification = new PaymentStatusNotification
            {
                OrderId = Guid.NewGuid(),
                TableId = Guid.NewGuid(),
                TableName = "Table 3",
                OldStatus = PaymentStatusEnums.Pending,
                NewStatus = PaymentStatusEnums.Paid,
                PaymentMethod = PaymentMethodEnums.COD,
                TotalAmount = 200000,
                UpdatedAt = DateTime.UtcNow
            };

            await _notificationService.SendPaymentStatusNotificationAsync(notification);
            return Ok(new { message = "Payment status notification sent", notification });
        }

        /// <summary>
        /// Test table notification
        /// </summary>
        [HttpPost("test-table-notification/{tableId}")]
        public async Task<IActionResult> TestTableNotification(Guid tableId, [FromBody] string message)
        {
            await _notificationService.SendTableNotificationAsync(tableId, message, "TestNotification");
            return Ok(new { message = "Table notification sent", tableId, content = message });
        }

        /// <summary>
        /// Test kitchen notification
        /// </summary>
        [HttpPost("test-kitchen-notification")]
        public async Task<IActionResult> TestKitchenNotification([FromBody] string message)
        {
            await _notificationService.SendKitchenNotificationAsync(message, "TestKitchenNotification");
            return Ok(new { message = "Kitchen notification sent", content = message });
        }

        /// <summary>
        /// Test waiter notification
        /// </summary>
        [HttpPost("test-waiter-notification")]
        public async Task<IActionResult> TestWaiterNotification([FromBody] string message)
        {
            await _notificationService.SendWaiterNotificationAsync(message, "TestWaiterNotification");
            return Ok(new { message = "Waiter notification sent", content = message });
        }

        /// <summary>
        /// Test moderator notification
        /// </summary>
        [HttpPost("test-moderator-notification")]
        public async Task<IActionResult> TestModeratorNotification([FromBody] string message)
        {
            await _notificationService.SendModeratorNotificationAsync(message, "TestModeratorNotification");
            return Ok(new { message = "Moderator notification sent", content = message });
        }
    }
}
