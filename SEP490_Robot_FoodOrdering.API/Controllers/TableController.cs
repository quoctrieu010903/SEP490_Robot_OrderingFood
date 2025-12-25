using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Application.Service.Implementation;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly ITableService _service;
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;
        
        public TableController(ITableService service, IOrderService orderService, IUnitOfWork unitOfWork)
        {
            _service = service;
            _orderService = orderService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTableRequest request)
        {
            var result = await _service.Create(request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.Delete(id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieve a paginated list of tables with optional status filtering.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of tables with support for:
        /// 
        /// * Pagination (page index and page size)
        /// * Filtering by table status
        /// 
        /// Sample request:
        /// GET /api/Table?PageIndex=1&PageSize=10&status=Available
        /// 
        /// Available table statuses:
        /// * Available � The table is free and ready to use
        /// * Occupied � The table is currently in use
        /// * Reserved � The table has been reserved
        /// 
        /// Use this endpoint to:
        /// * Display tables to staff or customers
        /// * Monitor real-time table status
        /// * Build dashboard or booking views
        /// </remarks>
        /// <param name="paging">Paging parameters (PageIndex, PageSize)</param>
        /// <param name="status">Optional status filter (Available, Occupied, Reserved)</param>
        /// <returns>A paginated list of tables matching the filter</returns>
        /// <response code="200">List of tables retrieved successfully</response>
        /// <response code="400">Invalid query parameters</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagingRequestModel paging, [FromQuery] TableEnums? status, [FromQuery] string? tableName)
        {
            var result = await _service.GetAll(paging, status, tableName);
            return Ok(result);
        }



        [HttpGet("{id}")]

        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetById(id);
            return Ok(result);
        }
        [HttpGet("{id}/scanQrCode/{DevidedId}")]
        public async Task<IActionResult> ScanQrCode(Guid id, string DevidedId)
        {
           // var result = await _service.ScanQrCode(id, DevidedId);
           var result = await _service.ScanQrCode01(id, DevidedId);
            return Ok(result);
        }
        // [HttpGet("{id}/scanQrCode01/{DevidedId}")]
        // public async Task<IActionResult> ScanQrCodeTest(Guid id, string DevidedId)
        // {
        //     var result = await _service.ScanQrCode01(id, DevidedId);
        //     return Ok(result);
        // }
        // Endpoint to share table and get QR code
        [HttpPost("{tableId}/share")]
        public async Task<ActionResult<BaseResponseModel<QrShareResponse>>> ShareTable(Guid tableId, [FromQuery] string currentDeviceId)
        {
            var result = await _service.ShareTableAsync(tableId, currentDeviceId);
            return Ok(result);
        }

        // Endpoint to accept shared table using QR token
        [HttpPost("{tableId}/accept-share")]
        public async Task<ActionResult<BaseResponseModel<TableResponse>>> AcceptSharedTable(Guid tableId, [FromQuery] string shareToken, [FromQuery] string newDeviceId)
        {
            var result = await _service.AcceptSharedTableAsync(tableId, shareToken, newDeviceId);
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStatusTable request)
        {
            var result = await _service.ChangeTableStatus(id, request.Status, request.Reason);
            return Ok(result);
        }
        [HttpPatch("{id}/Checkout")]
        public async Task<IActionResult> CheckoutTable(Guid id)
        {
            var result = await _service.CheckoutTable(id);
            return Ok(result);
        }
        /// <summary>
        /// Move the latest order from one table to another table.
        /// </summary>
        /// <remarks>
        /// This endpoint moves the most recent order from the old table to a new table.
        /// 
        /// Business Rules:
        /// * Old table must be in Occupied status
        /// * New table must be in Available status
        /// * Only the latest order (by CreatedTime) will be moved
        /// * All associated data will be transferred: Order, Invoice, TableSession, DeviceId
        /// * Old table will be set back to Available status
        /// * Reason is required for audit trail
        /// 
        /// Sample request:
        /// 
        ///     POST /api/Table/{oldTableId}/move
        ///     {
        ///         "newTableId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "reason": "Khách yêu cầu đổi sang bàn rộng hơn"
        ///     }
        /// 
        /// </remarks>
        /// <param name="oldTableId">The ID of the old table (must be Occupied)</param>
        /// <param name="request">Move table request containing newTableId and reason</param>
        /// <returns>The result of the operation with new table information</returns>
        /// <response code="200">Table moved successfully</response>
        /// <response code="400">Invalid request parameters (table status invalid, no orders found, etc.)</response>
        /// <response code="404">Old table or new table not found</response>
        /// <response code="409">Conflict - new order being created, please wait and retry</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("{oldTableId}/move")]
        public async Task<IActionResult> MoveTable(Guid oldTableId, [FromBody] MoveTableRequest request)
        {
            var result = await _service.MoveTable(oldTableId, request);
            return Ok(result);
        }

        /// <summary>
        /// Check if a device token matches the table's current device
        /// </summary>
        /// <remarks>
        /// This endpoint verifies whether the provided device token matches the device currently associated with the table.
        /// 
        /// Use cases:
        /// * Validate device ownership before allowing order operations
        /// * Check if device still has permission to modify orders
        /// * Detect if device session has expired or been taken over
        /// 
        /// Sample request:
        /// 
        ///     GET /api/Table/{tableId}/checkDeviceToken/{deviceId}
        /// 
        /// The response includes:
        /// * isMatch: true if device matches, false otherwise
        /// * Table information (name, status, isQrLocked)
        /// * Current device ID for comparison
        /// 
        /// </remarks>
        /// <param name="tableId">The ID of the table to check</param>
        /// <param name="deviceId">The device ID/token to verify</param>
        /// <returns>CheckDeviceTokenResponse with match result and table information</returns>
        /// <response code="200">Check completed successfully (returns isMatch=true/false)</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{tableId}/checkDeviceToken/{deviceId}")]
        public async Task<IActionResult> CheckTableAndDeviceToken(Guid tableId, string deviceId)
        {
            var result = await _service.CheckTableAndDeviceToken(tableId, deviceId);
            return Ok(result);
        }

        /// <summary>
        /// Random script to scan a table and create an order with random products (no toppings).
        /// </summary>
        /// <remarks>
        /// This endpoint performs the following steps:
        /// 1. Generates a random deviceId
        /// 2. Scans the specified table (or a random available table if tableId is not provided)
        /// 3. Randomly selects 1-3 products from the database
        /// 4. For each product, randomly selects a product size
        /// 5. Creates an order with no toppings
        /// 
        /// Sample request:
        /// GET /api/Table/random-scan-and-order
        /// GET /api/Table/random-scan-and-order?tableId=3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// </remarks>
        /// <param name="tableId">Optional table ID. If not provided, a random available table will be selected.</param>
        /// <returns>Result containing scan result and order creation result</returns>
        /// <response code="200">Operation completed successfully</response>
        /// <response code="400">Invalid request or no available tables/products found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("random-scan-and-order")]
        public async Task<IActionResult> RandomScanAndOrder([FromQuery] Guid? tableId = null)
        {
            try
            {
                var random = new Random();
                
                // Step 1: Generate random deviceId
                var randomDeviceId = Guid.NewGuid().ToString();

                // Step 2: Get table (random or specified)
                Guid selectedTableId;
                if (tableId.HasValue)
                {
                    selectedTableId = tableId.Value;
                    var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(selectedTableId);
                    if (table == null)
                    {
                        return BadRequest(new BaseResponseModel<object>(
                            StatusCodes.Status400BadRequest,
                            "TABLE_NOT_FOUND",
                            "Table not found"));
                    }
                }
                else
                {
                    // Get a random available table
                    var availableTables = (await _unitOfWork.Repository<Table, Guid>()
                        .GetListAsync(t => t.Status == TableEnums.Available && !t.DeletedTime.HasValue)).ToList();
                    
                    if (availableTables == null || !availableTables.Any())
                    {
                        return BadRequest(new BaseResponseModel<object>(
                            StatusCodes.Status400BadRequest,
                            "NO_AVAILABLE_TABLES",
                            "No available tables found"));
                    }
                    
                    selectedTableId = availableTables[random.Next(availableTables.Count)].Id;
                }

                // Step 2: Scan QR Code
                var scanResult = await _service.ScanQrCode01(selectedTableId, randomDeviceId);
                if (scanResult.StatusCode != StatusCodes.Status200OK)
                {
                    return StatusCode(scanResult.StatusCode, scanResult);
                }

                // Step 3: Get all products from database
                var allProducts = (await _unitOfWork.Repository<Product, Guid>()
                    .GetListAsync(p => !p.DeletedTime.HasValue)).ToList();
                
                if (allProducts == null || !allProducts.Any())
                {
                    return BadRequest(new BaseResponseModel<object>(
                        StatusCodes.Status400BadRequest,
                        "NO_PRODUCTS",
                        "No products found in database"));
                }

                // Step 4: Randomly select 1-3 products
                var numberOfItems = random.Next(1, 4); // 1 to 3 items
                var selectedProducts = allProducts.OrderBy(x => random.Next()).Take(numberOfItems).ToList();

                // Step 5: For each product, get a random product size
                var orderItems = new List<CreateOrderItemRequest>();
                foreach (var product in selectedProducts)
                {
                    var productSizes = (await _unitOfWork.Repository<ProductSize, Guid>()
                        .GetListAsync(ps => ps.ProductId == product.Id && !ps.DeletedTime.HasValue)).ToList();
                    
                    if (productSizes == null || !productSizes.Any())
                    {
                        continue; // Skip products without sizes
                    }

                    var randomSize = productSizes[random.Next(productSizes.Count)];
                    
                    orderItems.Add(new CreateOrderItemRequest
                    {
                        ProductId = product.Id,
                        ProductSizeId = randomSize.Id,
                        ToppingIds = new List<Guid>(), // No toppings as requested
                        Note = null
                    });
                }

                if (!orderItems.Any())
                {
                    return BadRequest(new BaseResponseModel<object>(
                        StatusCodes.Status400BadRequest,
                        "NO_VALID_PRODUCTS",
                        "No products with valid sizes found"));
                }

                // Step 6: Create order
                var createOrderRequest = new CreateOrderRequest
                {
                    TableId = selectedTableId,
                    deviceToken = randomDeviceId,
                    Items = orderItems
                };

                var orderResult = await _orderService.HandleOrderAsync(createOrderRequest);

                // Step 7: Set payment status to Paid for order and all order items
                if (orderResult.StatusCode == StatusCodes.Status200OK || orderResult.StatusCode == StatusCodes.Status201Created)
                {
                    if (orderResult.Data != null && orderResult.Data.Id != Guid.Empty)
                    {
                        var orderId = orderResult.Data.Id;
                        
                        // Load order with order items
                        var order = await _unitOfWork.Repository<Order, Guid>()
                            .GetByIdWithIncludeAsync(o => o.Id == orderId, true, o => o.OrderItems);
                        
                        if (order != null)
                        {
                            // Set payment status to Paid for order
                            order.PaymentStatus = PaymentStatusEnums.Paid;
                            order.LastUpdatedTime = DateTime.UtcNow;
                            
                            // Set payment status to Paid for all order items
                            foreach (var orderItem in order.OrderItems)
                            {
                                orderItem.PaymentStatus = PaymentStatusEnums.Paid;
                                orderItem.LastUpdatedTime = DateTime.UtcNow;
                                _unitOfWork.Repository<OrderItem, Guid>().Update(orderItem);
                            }
                            
                            // Update order
                            _unitOfWork.Repository<Order, Guid>().Update(order);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                // Return combined result
                var response = new
                {
                    DeviceId = randomDeviceId,
                    TableId = selectedTableId,
                    ScanResult = scanResult,
                    OrderResult = orderResult
                };

                return Ok(new BaseResponseModel<object>(
                    StatusCodes.Status200OK,
                    "SUCCESS",
                    response,
                    "Random scan and order created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponseModel<object>(
                        StatusCodes.Status500InternalServerError,
                        "INTERNAL_ERROR",
                        ex.Message));
            }
        }
    }
}