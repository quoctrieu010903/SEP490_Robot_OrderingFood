using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Application.Service.Implementation;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly ITableService _service;
        public TableController(ITableService service)
        {
            _service = service;
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
    }
}