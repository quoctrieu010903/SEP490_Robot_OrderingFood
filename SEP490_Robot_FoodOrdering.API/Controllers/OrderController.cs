using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using System;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Create a new order with items.
        /// </summary>
        /// <param name="request">Order creation request</param>
        /// <returns>Order details</returns>
        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get order details by order ID.
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetOrderById(Guid id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get a list of all orders.
        /// </summary>
        /// <returns>List of orders</returns>
        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetOrders()
        {
            var result = await _orderService.GetOrdersAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get all items for a specific order.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>List of order items</returns>
        [HttpGet("{orderId}/items")]
        public async Task<ActionResult<BaseResponseModel>> GetOrderItems(Guid orderId)
        {
            var result = await _orderService.GetOrderItemsAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Update the status of a specific order item.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="itemId">Order Item ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Updated order item</returns>
        [HttpPatch("{orderId}/items/{itemId}/status")]
        public async Task<ActionResult<BaseResponseModel>> UpdateOrderItemStatus(Guid orderId, Guid itemId, [FromBody] UpdateOrderItemStatusRequest request)
        {
            var result = await _orderService.UpdateOrderItemStatusAsync(orderId, itemId, request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Initiate payment for an order.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="request">Payment request</param>
        /// <returns>Payment status and details</returns>
        [HttpPost("{orderId}/pay")]
        public async Task<ActionResult<BaseResponseModel>> InitiatePayment(Guid orderId, [FromBody] OrderPaymentRequest request)
        {
            var result = await _orderService.InitiatePaymentAsync(orderId, request);
            return StatusCode(result.StatusCode, result);
        }
    }
} 