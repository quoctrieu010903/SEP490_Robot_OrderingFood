using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    /// <summary>
    /// Order management API endpoints for creating, retrieving, and managing food orders.
    /// </summary>
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
        /// <remarks>
        /// This endpoint creates a new order with the specified items, toppings, and table assignment.
        /// The order will be created with "Pending" status and the table will be marked as "Occupied".
        /// 
        /// Sample request:
        /// POST /api/Order
        /// {
        ///   "tableId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "items": [
        ///     {
        ///       "productId": "aedccca1-7419-4884-b3d1-e3fbdc87c97f",
        ///       "productSizeId": "c6fdcd9b-9691-4df2-a4c1-6160e0c54c21",
        ///       "toppingIds": [
        ///         "6039b485-d82a-45b1-aa06-37f53fef7c19"
        ///       ]
        ///     }
        ///   ]
        /// }
        /// 
        /// This endpoint supports:
        /// * Creating orders with multiple items
        /// * Adding toppings to order items
        /// * Automatic table status management
        /// * Total price calculation
        /// </remarks>
        /// <param name="request">Order creation request containing table ID and order items</param>
        /// <returns>Created order details with status and total price</returns>
        /// <response code="201">Order created successfully</response>
        /// <response code="400">Invalid request data or missing items</response>
        /// <response code="404">Table, product, or topping not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponseModel<OrderResponse>), 201)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 400)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 404)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Handle order creation or update existing pending order.
        /// </summary>
        /// <remarks>
        /// This endpoint intelligently handles order creation by checking for existing pending orders.
        /// If a pending order exists for the table, it adds new items to the existing order.
        /// If no pending order exists, it creates a new order.
        /// 
        /// Sample request:
        /// POST /api/Order/handle
        /// {
        ///   "tableId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "items": [
        ///     {
        ///       "productId": "aedccca1-7419-4884-b3d1-e3fbdc87c97f",
        ///       "productSizeId": "c6fdcd9b-9691-4df2-a4c1-6160e0c54c21",
        ///       "toppingIds": [
        ///         "6039b485-d82a-45b1-aa06-37f53fef7c19"
        ///       ]
        ///     }
        ///   ]
        /// }
        /// 
        /// This endpoint supports:
        /// * Intelligent order management (create or update)
        /// * Consolidating multiple orders for the same table
        /// * Automatic total price recalculation
        /// * Preventing duplicate pending orders
        /// </remarks>
        /// <param name="request">Order creation request containing table ID and order items</param>
        /// <returns>Order details (new or updated)</returns>
        /// <response code="200">Order updated successfully</response>
        /// <response code="201">New order created successfully</response>
        /// <response code="400">Invalid request data or missing items</response>
        /// <response code="404">Table, product, or topping not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("handle")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderResponse>), 200)]
        [ProducesResponseType(typeof(BaseResponseModel<OrderResponse>), 201)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 400)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 404)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> HandleOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.HandleOrderAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get order details by order ID.
        /// </summary>
        /// <remarks>
        /// Retrieves detailed information about a specific order including all items, toppings, and status.
        /// 
        /// Sample request:
        /// GET /api/Order/{id}
        /// 
        /// This endpoint supports:
        /// * Retrieving complete order details
        /// * Order item information with toppings
        /// * Order status and payment information
        /// * Table assignment details
        /// </remarks>
        /// <param name="id">Unique identifier of the order</param>
        /// <returns>Detailed order information</returns>
        /// <response code="200">Order found and returned successfully</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderResponse>), 200)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 404)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> GetOrderById(Guid id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get a list of all orders with pagination.
        /// </summary>
        /// <remarks>
        /// Retrieves a paginated list of all orders in the system.
        /// 
        /// Sample request:
        /// GET /api/Order?pageNumber=1&pageSize=10
        /// 
        /// This endpoint supports:
        /// * Pagination with configurable page size
        /// * Order status filtering
        /// * Payment status filtering
        /// * Sorting by creation date, total price
        /// </remarks>
        /// <param name="paging">Pagination parameters (pageNumber, pageSize)</param>
        /// <returns>Paginated list of orders</returns>
        /// <response code="200">Orders retrieved successfully</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<OrderResponse>), 200)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 400)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> GetOrders([FromQuery] PagingRequestModel paging)
        {
            var result = await _orderService.GetOrdersAsync(paging);
            return StatusCode(200, result);
        }

        /// <summary>
        /// Get all items for a specific order.
        /// </summary>
        /// <remarks>
        /// Retrieves detailed information about all items in a specific order.
        /// 
        /// Sample request:
        /// GET /api/Order/{orderId}/items
        /// 
        /// This endpoint supports:
        /// * Retrieving all order items
        /// * Item status information
        /// * Topping details for each item
        /// * Product and size information
        /// </remarks>
        /// <param name="orderId">Unique identifier of the order</param>
        /// <returns>List of order items with details</returns>
        /// <response code="200">Order items retrieved successfully</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{orderId}/items")]
        [ProducesResponseType(typeof(BaseResponseModel<List<OrderItemResponse>>), 200)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 404)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> GetOrderItems(Guid orderId)
        {
            var result = await _orderService.GetOrderItemsAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Update the status of a specific order item.
        /// </summary>
        /// <remarks>
        /// Updates the status of a specific item within an order (e.g., Pending → Preparing → Ready → Served).
        /// 
        /// Sample request:
        /// PATCH /api/Order/{orderId}/items/{itemId}/status
        /// {
        ///   "status": "Preparing"
        /// }
        /// 
        /// Available statuses:
        /// * Pending (1)
        /// * Preparing (2)
        /// * Ready (3)
        /// * Served (4)
        /// * Completed (5)
        /// * Cancelled (6)
        /// 
        /// This endpoint supports:
        /// * Individual item status updates
        /// * Order status recalculation
        /// * Kitchen workflow management
        /// </remarks>
        /// <param name="orderId">Unique identifier of the order</param>
        /// <param name="itemId">Unique identifier of the order item</param>
        /// <param name="request">Status update request</param>
        /// <returns>Updated order item information</returns>
        /// <response code="200">Item status updated successfully</response>
        /// <response code="400">Invalid status or request data</response>
        /// <response code="404">Order or item not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPatch("{orderId}/items/{itemId}/status")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderItemResponse>), 200)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 400)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 404)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> UpdateOrderItemStatus(Guid orderId, Guid itemId, [FromBody] UpdateOrderItemStatusRequest request)
        {
            var result = await _orderService.UpdateOrderItemStatusAsync(orderId, itemId, request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get orders by table ID.
        /// </summary>
        /// <remarks>
        /// Retrieves all orders associated with a specific table.
        /// 
        /// Sample request:
        /// GET /api/Order/{orderId}/table/{tableId}
        /// 
        /// This endpoint supports:
        /// * Table-specific order retrieval
        /// * Multiple orders per table
        /// * Order history for table management
        /// </remarks>
        /// <param name="orderId">Unique identifier of the order</param>
        /// <param name="tableId">Unique identifier of the table</param>
        /// <returns>List of orders for the specified table</returns>
        /// <response code="200">Orders retrieved successfully</response>
        /// <response code="404">Table or order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{orderId}/table/{tableId}")]
        [ProducesResponseType(typeof(BaseResponseModel<List<OrderResponse>>), 200)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 404)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> GetOrdersByTableId(Guid orderId, Guid tableId )
        {
            var result = await _orderService.GetOrdersbyTableiDAsync(orderId, tableId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get all orders for a specific table (for payment).
        /// </summary>
        /// <remarks>
        /// Retrieves all orders associated with a specific table for payment processing.
        /// 
        /// Sample request:
        /// GET /api/Order/table/{tableId}
        /// 
        /// This endpoint supports:
        /// * Table-specific order retrieval for payment
        /// * Multiple orders per table
        /// * Complete order details with items and prices
        /// </remarks>
        /// <param name="tableId">Unique identifier of the table</param>
        /// <returns>List of orders for the specified table</returns>
        /// <response code="200">Orders retrieved successfully</response>
        /// <response code="404">Table not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("table/{tableId}")]
        [ProducesResponseType(typeof(BaseResponseModel<List<OrderResponse>>), 200)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 404)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> GetOrdersByTableIdOnly(Guid tableId)
        {
            var result = await _orderService.GetOrdersByTableIdOnlyAsync(tableId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get orders by table ID with specific status (for payment).
        /// </summary>
        /// <remarks>
        /// Retrieves orders associated with a specific table and status for payment processing.
        /// 
        /// Sample request:
        /// GET /api/Order/table/{tableId}/status/{status}
        /// 
        /// This endpoint supports:
        /// * Table-specific order retrieval with status filtering
        /// * Status-based filtering (e.g., Delivering orders only)
        /// * Complete order details with items and prices
        /// </remarks>
        /// <param name="tableId">Unique identifier of the table</param>
        /// <param name="status">Order status to filter by</param>
        /// <returns>List of orders for the specified table and status</returns>
        /// <response code="200">Orders retrieved successfully</response>
        /// <response code="404">Table not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("table/{tableId}/status/{status}")]
        [ProducesResponseType(typeof(BaseResponseModel<List<OrderResponse>>), 200)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 404)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> GetOrdersByTableIdWithStatus(Guid tableId, OrderStatus status)
        {
            var result = await _orderService.GetOrdersByTableIdWithStatusAsync(tableId, status);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Initiate payment for an order.
        /// </summary>
        /// <remarks>
        /// Initiates the payment process for a specific order with the specified payment method.
        /// 
        /// Sample request:
        /// POST /api/Order/{orderId}/pay
        /// {
        ///   "paymentMethod": "VNPay"
        /// }
        /// 
        /// Available payment methods:
        /// * COD (Cash on Delivery)
        /// * VNPay (Online payment)
        /// 
        /// This endpoint supports:
        /// * Multiple payment methods
        /// * Payment status tracking
        /// * Payment URL generation for online payments
        /// * Order status updates after payment
        /// </remarks>
        /// <param name="orderId">Unique identifier of the order</param>
        /// <param name="request">Payment request with payment method</param>
        /// <returns>Payment status and details</returns>
        /// <response code="200">Payment initiated successfully</response>
        /// <response code="400">Invalid payment method or request data</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("{orderId}/pay")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 400)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 404)]
        [ProducesResponseType(typeof(BaseResponseModel<object>), 500)]
        public async Task<ActionResult<BaseResponseModel>> InitiatePayment(Guid orderId, [FromBody] OrderPaymentRequest request)
        {
            var result = await _orderService.InitiatePaymentAsync(orderId, request);
            return StatusCode(result.StatusCode, result);
        }
    }
} 