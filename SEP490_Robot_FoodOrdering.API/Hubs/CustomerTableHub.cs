using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.API.Hubs
{
    /// <summary>
    /// SignalR Hub for customer table operations
    /// Supports per-table operations: handle order, get orders, get complains, change table
    /// Route: /hubs/customer-table
    /// Usage: Connect to /hubs/customer-table/{tableId} and call methods with tableId parameter
    /// </summary>
    public class CustomerTableHub : Hub
    {
        private readonly IOrderService _orderService;
        private readonly IComplainService _complainService;
        private readonly ITableService _tableService;
        private readonly ILogger<CustomerTableHub> _logger;

        public CustomerTableHub(
            IOrderService orderService,
            IComplainService complainService,
            ITableService tableService,
            ILogger<CustomerTableHub> logger)
        {
            _orderService = orderService;
            _complainService = complainService;
            _tableService = tableService;
            _logger = logger;
        }

        /// <summary>
        /// Handle order creation or update existing pending order
        /// Equivalent to POST /api/Order/handle
        /// </summary>
        /// <param name="tableId">Table ID from route</param>
        /// <param name="request">Order creation request</param>
        /// <returns>Order response</returns>
        public async Task<object> HandleOrder(string tableId, CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "CustomerTableHub.HandleOrder: tableId={TableId}, itemsCount={ItemsCount}",
                    tableId, request?.Items?.Count ?? 0);

                // Validate tableId from route matches request
                if (!Guid.TryParse(tableId, out var parsedTableId))
                {
                    return (object)new BaseResponseModel<object>(
                        StatusCodes.Status400BadRequest,
                        "INVALID_TABLE_ID",
                        null,
                        null,
                        "Invalid table ID format");
                }

                if (request == null)
                {
                    return (object)new BaseResponseModel<object>(
                        StatusCodes.Status400BadRequest,
                        "INVALID_REQUEST",
                        null,
                        null,
                        "Request cannot be null");
                }

                // Ensure tableId in request matches route
                request.TableId = parsedTableId;

                var result = await _orderService.HandleOrderAsync(request);

                // Send response back to caller
                await Clients.Caller.SendAsync("HandleOrderResponse", result);

                return (object)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CustomerTableHub.HandleOrder: Error for tableId={TableId}", tableId);
                var errorResponse = new BaseResponseModel<object>(
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR",
                    null,
                    null,
                    ex.Message);
                await Clients.Caller.SendAsync("HandleOrderResponse", errorResponse);
                return (object)errorResponse;
            }
        }

        /// <summary>
        /// Get orders by table ID
        /// Equivalent to GET /api/Order/table/{tableId}
        /// </summary>
        /// <param name="tableId">Table ID from route</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>List of orders</returns>
        public async Task<object> GetOrdersByTable(
            string tableId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation(
                    "CustomerTableHub.GetOrdersByTable: tableId={TableId}, startDate={StartDate}, endDate={EndDate}",
                    tableId, startDate, endDate);

                if (!Guid.TryParse(tableId, out var parsedTableId))
                {
                    return (object)new BaseResponseModel<object>(
                        StatusCodes.Status400BadRequest,
                        "INVALID_TABLE_ID",
                        null,
                        null,
                        "Invalid table ID format");
                }

                var result = await _orderService.GetOrdersByTableIdOnlyAsync(parsedTableId, startDate, endDate);

                // Send response back to caller
                await Clients.Caller.SendAsync("GetOrdersByTableResponse", result);

                return (object)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CustomerTableHub.GetOrdersByTable: Error for tableId={TableId}", tableId);
                var errorResponse = new BaseResponseModel<object>(
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR",
                    null,
                    null,
                    ex.Message);
                await Clients.Caller.SendAsync("GetOrdersByTableResponse", errorResponse);
                return (object)errorResponse;
            }
        }

        /// <summary>
        /// Get complains by table ID
        /// Equivalent to GET /api/Complain/{idTable}?isCustomer=true
        /// </summary>
        /// <param name="tableId">Table ID from route</param>
        /// <param name="isCustomer">Filter for customer view (default: true)</param>
        /// <returns>List of complains</returns>
        public async Task<object> GetComplainsByTable(string tableId, bool isCustomer = true)
        {
            try
            {
                _logger.LogInformation(
                    "CustomerTableHub.GetComplainsByTable: tableId={TableId}, isCustomer={IsCustomer}",
                    tableId, isCustomer);

                if (!Guid.TryParse(tableId, out var parsedTableId))
                {
                    return (object)new BaseResponseModel<object>(
                        StatusCodes.Status400BadRequest,
                        "INVALID_TABLE_ID",
                        null,
                        null,
                        "Invalid table ID format");
                }

                var result = await _complainService.GetComplainByTable(parsedTableId, isCustomer);

                // Send response back to caller
                await Clients.Caller.SendAsync("GetComplainsByTableResponse", result);

                return (object)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CustomerTableHub.GetComplainsByTable: Error for tableId={TableId}", tableId);
                var errorResponse = new BaseResponseModel<object>(
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR",
                    null,
                    null,
                    ex.Message);
                await Clients.Caller.SendAsync("GetComplainsByTableResponse", errorResponse);
                return (object)errorResponse;
            }
        }

        /// <summary>
        /// Change table (move table)
        /// Equivalent to POST /api/Table/{oldTableId}/move
        /// </summary>
        /// <param name="tableId">Current table ID from route</param>
        /// <param name="request">Move table request</param>
        /// <returns>Table response</returns>
        public async Task<object> ChangeTable(string tableId, MoveTableRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "CustomerTableHub.ChangeTable: oldTableId={OldTableId}, newTableId={NewTableId}, reason={Reason}",
                    tableId, request?.NewTableId, request?.Reason);

                if (!Guid.TryParse(tableId, out var parsedTableId))
                {
                    return (object)new BaseResponseModel<object>(
                        StatusCodes.Status400BadRequest,
                        "INVALID_TABLE_ID",
                        null,
                        null,
                        "Invalid table ID format");
                }

                if (request == null)
                {
                    return (object)new BaseResponseModel<object>(
                        StatusCodes.Status400BadRequest,
                        "INVALID_REQUEST",
                        null,
                        null,
                        "Request cannot be null");
                }

                var result = await _tableService.MoveTable(parsedTableId, request);

                // Send response back to caller
                await Clients.Caller.SendAsync("ChangeTableResponse", result);

                return (object)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CustomerTableHub.ChangeTable: Error for tableId={TableId}", tableId);
                var errorResponse = new BaseResponseModel<object>(
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR",
                    null,
                    null,
                    ex.Message);
                await Clients.Caller.SendAsync("ChangeTableResponse", errorResponse);
                return (object)errorResponse;
            }
        }

        /// <summary>
        /// Join a table-specific group for receiving notifications
        /// </summary>
        /// <param name="tableId">Table ID to join</param>
        public async Task JoinTableGroup(string tableId)
        {
            if (!string.IsNullOrEmpty(tableId))
            {
                // Normalize tableId to lowercase for consistent group naming
                var normalizedTableId = tableId.ToLowerInvariant().Trim();
                await Groups.AddToGroupAsync(Context.ConnectionId, $"CustomerTable_{normalizedTableId}");
                _logger.LogInformation(
                    "CustomerTableHub: Client {ConnectionId} joined table group CustomerTable_{TableId}",
                    Context.ConnectionId, normalizedTableId);
            }
        }

        /// <summary>
        /// Leave a table-specific group
        /// </summary>
        /// <param name="tableId">Table ID to leave</param>
        public async Task LeaveTableGroup(string tableId)
        {
            if (!string.IsNullOrEmpty(tableId))
            {
                // Normalize tableId to lowercase for consistent group naming
                var normalizedTableId = tableId.ToLowerInvariant().Trim();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"CustomerTable_{normalizedTableId}");
                _logger.LogInformation(
                    "CustomerTableHub: Client {ConnectionId} left table group CustomerTable_{TableId}",
                    Context.ConnectionId, normalizedTableId);
            }
        }

        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "CustomerTableHub: Client {ConnectionId} connected",
                Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation(
                "CustomerTableHub: Client {ConnectionId} disconnected",
                Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}

