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

        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetOrderById(Guid id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetOrders()
        {
            var result = await _orderService.GetOrdersAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{orderId}/items")]
        public async Task<ActionResult<BaseResponseModel>> GetOrderItems(Guid orderId)
        {
            var result = await _orderService.GetOrderItemsAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{orderId}/items/{itemId}/status")]
        public async Task<ActionResult<BaseResponseModel>> UpdateOrderItemStatus(Guid orderId, Guid itemId, [FromBody] UpdateOrderItemStatusRequest request)
        {
            var result = await _orderService.UpdateOrderItemStatusAsync(orderId, itemId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{orderId}/pay")]
        public async Task<ActionResult<BaseResponseModel>> InitiatePayment(Guid orderId, [FromBody] OrderPaymentRequest request)
        {
            var result = await _orderService.InitiatePaymentAsync(orderId, request);
            return StatusCode(result.StatusCode, result);
        }
    }
} 