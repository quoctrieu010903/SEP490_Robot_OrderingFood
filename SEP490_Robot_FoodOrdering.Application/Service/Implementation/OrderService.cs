    using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Notification;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Application.Mapping;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Specifications;
using SEP490_Robot_FoodOrdering.Application.DTO.Response;
using SEP490_Robot_FoodOrdering.Core.Constants;


namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IOrderItemReposotory _orderItemReposotory;
        private readonly INotificationService? _notificationService;
        private readonly IRemakeItemService _remakeItemService;
        private readonly ICancelledItemService _cancelledItemService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger, IOrderItemReposotory orderItemReposotory, INotificationService? notificationService, ICancelledItemService cancelledItemService, IRemakeItemService remakeItemService ,IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _orderItemReposotory = orderItemReposotory;
            _notificationService = notificationService;
            _cancelledItemService = cancelledItemService;
            _remakeItemService = remakeItemService;
            _httpContextAccessor = httpContextAccessor;
        }


        #region Order Management old , need to improve 
        public async Task<BaseResponseModel<OrderResponse>> CreateOrderAsync(CreateOrderRequest request)
            {
            if (request.Items == null || !request.Items.Any())
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status400BadRequest, "NO_ITEMS",
                    "Order must have at least one item.");


            var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(request.TableId);
            if (table == null)
                throw new ErrorException(StatusCodes.Status400BadRequest, "TABLE_NOT_FOUND", "Table not found.");

            // ✅ THÊM MỚI: Kiểm tra trạng thái bàn
            if (table.Status != TableEnums.Occupied)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST,
                    "Chỉ có thể đặt món khi bàn đang được sử dụng");

            // ✅ THÊM MỚI: Kiểm tra device có quyền order không
            if (string.IsNullOrEmpty(request.deviceToken) || table.DeviceId != request.deviceToken)
                throw new ErrorException(StatusCodes.Status403Forbidden, ResponseCodeConstants.FORBIDDEN,
                    "Thiết bị không có quyền đặt món cho bàn này");

            var order = _mapper.Map<Order>(request);

            order.Status = OrderStatus.Pending;
            order.PaymentStatus = PaymentStatusEnums.Pending;
            order.CreatedBy = request.deviceToken;
            order.LastUpdatedBy = request.deviceToken;

            order.CreatedTime = DateTime.UtcNow;
            order.LastUpdatedTime = DateTime.UtcNow;

            order.OrderItems = new List<OrderItem>();
            decimal total = 0;
            foreach (var itemReq in request.Items)
            {
                var product = await _unitOfWork.Repository<Product, Guid>().GetByIdAsync(itemReq.ProductId);
                var productSize = await _unitOfWork.Repository<ProductSize, Guid>().GetByIdAsync(itemReq.ProductSizeId);
                if (product == null || productSize == null)
                    throw new ErrorException(StatusCodes.Status400BadRequest, "INVALID_PRODUCT_OR_SIZE",
                        "Invalid product or size.");

                var orderItem = new OrderItem
                {
                    ProductId = itemReq.ProductId,
                    Product = product,
                    ProductSizeId = itemReq.ProductSizeId,
                    ProductSize = productSize,
                    Price = productSize.Price,
                    Note = itemReq.Note,
                    Status = OrderItemStatus.Pending,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow,
                    OrderItemTopping = new List<OrderItemTopping>()
                };
                order.OrderItems.Add(orderItem);

                total += productSize.Price;

                foreach (var toppingId in itemReq.ToppingIds)
                {
                    var productTopping = await _unitOfWork.Repository<ProductTopping, Guid>()
                        .GetWithSpecAsync(
                            new BaseSpecification<ProductTopping>(pt =>
                                pt.ProductId == itemReq.ProductId && pt.ToppingId == toppingId), true);

                    if (productTopping == null)
                        throw new ErrorException(StatusCodes.Status400BadRequest, "INVALID_TOPPING",
                            $"Topping {toppingId} is not valid for Product {itemReq.ProductId}");

                    var topping = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(toppingId);
                    if (topping == null)
                        throw new ErrorException(StatusCodes.Status400BadRequest, "TOPPING_NOT_FOUND",
                            "Topping not found.");

                    orderItem.OrderItemTopping.Add(new OrderItemTopping
                    {
                        ToppingId = topping.Id,
                        Price = topping.Price,
                        CreatedTime = DateTime.UtcNow,
                        LastUpdatedTime = DateTime.UtcNow
                    });
                    

                    total += topping.Price;
                }
                   orderItem.TotalPrice = total;
               
            }

            order.TotalPrice = total;
           
            
            await _unitOfWork.Repository<Order, Guid>().AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            var response = _mapper.Map<OrderResponse>(order);
            return new BaseResponseModel<OrderResponse>(StatusCodes.Status201Created, "ORDER_CREATED", response);
            }
            
        public async Task<BaseResponseModel<OrderResponse>> HandleOrderAsync(CreateOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status400BadRequest, "NO_ITEMS",
                    "Order must have at least one item.");

            // 1. Tìm order đang pending của bàn này
            var existingOrder = await _unitOfWork.Repository<Order, Guid>()
                .GetWithSpecAsync(new OrderSpecification(request.TableId), true);

            // 2. Nếu có order -> thêm item vào và cập nhật lại giá
            if (existingOrder != null)
            {
                if (!existingOrder.Table.DeviceId.Equals(request.deviceToken))
                {
                    return new BaseResponseModel<OrderResponse>(StatusCodes.Status400BadRequest, "",
                        "Thiết bị không có quyền đặt hàng");
                }
               
                // Ensure table is marked as occupi ed when adding items to existing order
                var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(request.TableId);
                if (table != null && table.Status != TableEnums.Occupied)
                {
                    table.Status = TableEnums.Occupied;
                    _unitOfWork.Repository<Table, Guid>().Update(table);
                }

                decimal addedTotal = 0;

                foreach (var itemReq in request.Items)
                {
                    var product = await _unitOfWork.Repository<Product, Guid>().GetByIdAsync(itemReq.ProductId);
                    var productSize = await _unitOfWork.Repository<ProductSize, Guid>()
                        .GetByIdAsync(itemReq.ProductSizeId);
                    if (product == null || productSize == null)
                        throw new ErrorException(StatusCodes.Status400BadRequest, "INVALID_PRODUCT_OR_SIZE",
                            "Invalid product or size.");


                    var orderItem = new OrderItem
                    {
                        OrderId = existingOrder.Id,
                        ProductId = itemReq.ProductId,
                        ProductSizeId = itemReq.ProductSizeId,
                        Note = itemReq.Note,
                        Price = productSize.Price,
                        Status = OrderItemStatus.Pending,
                        CreatedTime = DateTime.UtcNow,
                        LastUpdatedTime = DateTime.UtcNow,
                        OrderItemTopping = new List<OrderItemTopping>()
                    };
                    existingOrder.OrderItems.Add(orderItem);
                    addedTotal += productSize.Price;
                    foreach (var toppingId in itemReq.ToppingIds)
                    {
                        var productTopping = await _unitOfWork.Repository<ProductTopping, Guid>()
                            .GetWithSpecAsync(
                                new BaseSpecification<ProductTopping>(pt =>
                                    pt.ProductId == itemReq.ProductId && pt.ToppingId == toppingId), true);

                        if (productTopping == null)
                            throw new ErrorException(StatusCodes.Status400BadRequest, "INVALID_TOPPING",
                                $"Topping {toppingId} is not valid for Product {itemReq.ProductId}");

                        var topping = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(toppingId);
                        if (topping == null)
                            throw new ErrorException(StatusCodes.Status400BadRequest, "TOPPING_NOT_FOUND",
                                "Topping not found.");

                        orderItem.OrderItemTopping.Add(new OrderItemTopping
                        {
                            ToppingId = topping.Id,
                            Price = topping.Price,
                            CreatedTime = DateTime.UtcNow,
                            LastUpdatedTime = DateTime.UtcNow
                        });

                        addedTotal += topping.Price;
                        
                    }
                    orderItem.TotalPrice = addedTotal;
                }

                existingOrder.TotalPrice += addedTotal;
                existingOrder.LastUpdatedTime = DateTime.UtcNow;

                _unitOfWork.Repository<Order, Guid>().Update(existingOrder);
                await _unitOfWork.SaveChangesAsync();

                var response = _mapper.Map<OrderResponse>(existingOrder);
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status200OK, "ORDER_UPDATED", response);
            }

            // 3. Nếu chưa có order pending -> tạo mới
            return await CreateOrderAsync(request);
        }
        #endregion

        public async Task<BaseResponseModel<OrderResponse>> GetOrderByIdAsync(Guid orderId
        )
        {
            var order = await _unitOfWork.Repository<Order, Guid>()
                .GetWithSpecAsync(new OrderSpecification(orderId, true), true);
            if (order == null)
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status404NotFound, "NOT_FOUND",
                    "Order not found.");
            var response = _mapper.Map<OrderResponse>(order);
            return new BaseResponseModel<OrderResponse>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<PaginatedList<OrderResponse>> GetOrdersAsync(PagingRequestModel paging, string? ProductName)
        {
            // Tính start và end theo múi giờ Việt Nam
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var todayVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).Date;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayVN, vnTimeZone);
            var endUtc = startUtc.AddDays(1);
            var specification = new OrderSpecification(ProductName, startUtc, endUtc);

            var orders = await _unitOfWork.Repository<Order, Order>().GetAllWithSpecAsync(specification, true);
                var response = _mapper.Map<List<OrderResponse>>(orders);


            // Group OrderItems by ProductName or Status for the UI grouping
            // foreach (var order in response)
            // {
            //     order.Items = order.Items
            //         .GroupBy(item => new { item.ProductName, item.Status }) // Group by ProductName and Status
            //         .Select(g => new OrderItemResponse
            //         {
            //             Id = g.First().Id,
            //             ProductId = g.First().ProductId,
            //             ProductName = g.Key.ProductName,
            //             ProductSizeId = g.First().ProductSizeId,
            //             SizeName = g.First().SizeName,
            //             Quantity = g.Sum(x => 1), // Count items in the group
            //             Price = g.First().Price * g.Count(), // Total price for the group
            //             Status = g.Key.Status,
            //             ImageUrl = g.First().ImageUrl,
            //             CreatedTime = g.Min(x => x.CreatedTime),
            //             Toppings = g.SelectMany(x => x.Toppings).Distinct().ToList() // Combine toppings
            //         }).ToList();
            // }

            return PaginatedList<OrderResponse>.Create(response, paging.PageNumber, paging.PageSize);
        }

        //public async Task<PaginatedList<OrderResponse>> GetOrdersAsync(PagingRequestModel paging)
        //{
        //    var orders = await _unitOfWork.Repository<Order, Order>().GetAllWithSpecAsync( new OrderSpecification(), true);
        //    var response = _mapper.Map<List<OrderResponse>>(orders);
        //    return  PaginatedList<OrderResponse>.Create(response, paging.PageNumber, paging.PageSize);      
        //}
        public async Task<BaseResponseModel<List<OrderResponse>>> GetOrdersbyTableiDAsync(Guid Orderid, Guid TableId)
        {
            var orders = await _unitOfWork.Repository<Order, Order>()
                .GetAllWithSpecAsync(new OrderSpecification(Orderid, TableId, true), true);
            var response = _mapper.Map<List<OrderResponse>>(orders);
            return new BaseResponseModel<List<OrderResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<List<OrderResponse>>> GetOrdersByTableIdOnlyAsync(Guid tableId , DateTime? startDate, DateTime? endDate)
            {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Normalize sang UTC
            DateTime? startUtc = startDate.HasValue
                ? TimeZoneInfo.ConvertTimeToUtc(startDate.Value.Date, vnTimeZone)
                : null;

            DateTime? endUtc = endDate.HasValue
                ? TimeZoneInfo.ConvertTimeToUtc(endDate.Value.Date.AddDays(1), vnTimeZone)
                : null;

            var orders = await _unitOfWork.Repository<Order, Order>()
                .GetAllWithSpecAsync(new OrderSpecification(tableId,startUtc,endUtc), true);
                var response = _mapper.Map<List<OrderResponse>>(orders);
            return new BaseResponseModel<List<OrderResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<List<OrderResponse>>> GetOrdersByTableIdWithStatusAsync(Guid tableId,
            OrderStatus status)
        {
            var orders = await _unitOfWork.Repository<Order, Order>()
                .GetAllWithSpecAsync(new OrderSpecification(tableId, status), true);
            

            // Debug: Log the orders to see what data is being returned
            foreach (var order in orders)
            {
                _logger.LogInformation($"Order {order.Id} has {order.OrderItems?.Count ?? 0} items");
                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        _logger.LogInformation(
                            $"OrderItem {item.Id}: ProductSize={item.ProductSize?.Id}, Price={item.ProductSize?.Price}, Product={item.Product?.Name}");
                    }
                }
            }

            var response = _mapper.Map<List<OrderResponse>>(orders);
            return new BaseResponseModel<List<OrderResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<OrderItemResponse>> UpdateOrderItemStatusAsync(Guid orderId,
            Guid orderItemId, UpdateOrderItemStatusRequest request)
        {
            // var userid = Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst("Id")?.Value);
            var order = await _unitOfWork.Repository<Order, Guid>()
                .GetWithSpecAsync(new OrderSpecification(orderId, true), true);
            if (order == null)
                return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND",
                    "Order not found.");
            var item = order.OrderItems.FirstOrDefault(i => i.Id == orderItemId);
            if (item == null)
                return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status404NotFound, "ITEM_NOT_FOUND",
                    "Order item not found.");

            if (request.Status == OrderItemStatus.Remark)
            {
                if (string.IsNullOrWhiteSpace(request.RemarkNote))
                {
                    return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status400BadRequest, "NOTE_REQUIRED",
                        "Note is required when status is Remark.");
                }

                item.RemakeNote = request.RemarkNote; // hoặc item.Note nếu bạn dùng field Note
                
            }
            if(request.Status == OrderItemStatus.Cancelled)
            {
                if(string.IsNullOrWhiteSpace(request.RemarkNote))
                {
                    return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status400BadRequest, "NOTE_REQUIRED",
                        "Note is required when status is Cancelled.");
                }   
               // await _cancelledItemService.CreateCancelledItemAsync(orderItemId, request.RemarkNote ?? "", userid);
            }
            if(request.Status == OrderItemStatus.Remark)
            {
                if (string.IsNullOrWhiteSpace(request.RemarkNote))
                {
                    return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status400BadRequest, "NOTE_REQUIRED",
                        "Note is required when status is Remarked.");
                }
                // Call remake service to create remake item
               // await _remakeItemService.CreateRemakeItemAsync(orderItemId, request.RemarkNote ?? "", userid);
            }

            var oldStatus = item.Status;
            // Determine which items to update.
            // For Cancelled or Remake, update ONLY the selected item.
            var isSingleItemOnly = request.Status == OrderItemStatus.Cancelled || request.Status == OrderItemStatus.Remark;
            var targets = isSingleItemOnly
                ? new List<OrderItem> { item }
                : order.OrderItems
                    .Where(i => i.ProductId == item.ProductId
                                && i.ProductSizeId == item.ProductSizeId
                                && i.Status == oldStatus)
                    .ToList();

            foreach (var oi in targets)
            {
                if (request.Status == OrderItemStatus.Remark)
                {
                    oi.RemakeNote = request.RemarkNote;
                }
                oi.Status = request.Status;
                oi.LastUpdatedTime = DateTime.UtcNow;
                _logger.LogInformation(
                    $"OrderItem {oi.Id} status changed from {oldStatus} to {oi.Status} in Order {orderId}");
            }
            _logger.LogInformation(
                $"Update applied: {targets.Count} item(s) for Product {item.ProductId} Size {item.ProductSizeId} changed from {oldStatus} to {request.Status} in Order {orderId}");
            // Update order status automatically
            var oldOrderStatus = order.Status;
            order.Status = await CalculateOrderStatusAsync(order.OrderItems, order.Table);
            order.TotalPrice = CalculateOrderTotal(order.OrderItems);

            order.LastUpdatedTime = DateTime.UtcNow;
            if (oldOrderStatus != order.Status)
            {
                _logger.LogInformation($"Order {orderId} status changed from {oldOrderStatus} to {order.Status}");
            }

            await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            
            // Send real-time notification for order item status change
            if (_notificationService != null)
            {
                try
                {
                    foreach (var updatedItem in targets)
                    {
                        var orderItemNotification = new OrderItemStatusNotification
                        {
                            OrderId = orderId,
                            OrderItemId = updatedItem.Id,
                            TableId = order.TableId ?? Guid.Empty,
                            TableName = order.Table?.Name ?? "Unknown",
                            ProductName = updatedItem.Product?.Name ?? "Unknown Product",
                            SizeName = updatedItem.ProductSize?.SizeName.ToString() ?? "Unknown Size",
                            OldStatus = oldStatus,
                            NewStatus = updatedItem.Status,
                            RemarkNote = updatedItem.RemakeNote,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = "System"
                        };

                        await _notificationService.SendOrderItemStatusNotificationAsync(orderItemNotification);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send order item status notification");
                    // Don't fail the main operation if notification fails
                }
            }

            // Send order status notification if order status changed
            if (oldOrderStatus != order.Status && _notificationService != null)
            {
                try
                {
                    var orderNotification = new OrderStatusNotification
                    {
                        OrderId = orderId,
                        TableId = order.TableId ?? Guid.Empty,
                        TableName = order.Table?.Name ?? "Unknown",
                        OldStatus = oldOrderStatus,
                        NewStatus = order.Status,
                        TotalPrice = order.TotalPrice,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = "System"
                    };

                    await _notificationService.SendOrderStatusNotificationAsync(orderNotification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send order status notification");
                    // Don't fail the main operation if notification fails
                }
            }
            
            var response = _mapper.Map<OrderItemResponse>(item);
            return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status200OK, "ITEM_STATUS_UPDATED", response);
        }

    
        public async Task<BaseResponseModel<List<OrderItemResponse>>> GetOrderItemsAsync(Guid orderId)
        {
            var order = await _unitOfWork.Repository<Order, Guid>()
                .GetWithSpecAsync(new OrderSpecification(orderId, true), true);
            if (order == null)
                return new BaseResponseModel<List<OrderItemResponse>>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND",
                    "Order not found.");
            var response = _mapper.Map<List<OrderItemResponse>>(order.OrderItems.ToList());
            return new BaseResponseModel<List<OrderItemResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        //public async Task<BaseResponseModel<OrderPaymentResponse>> InitiatePaymentAsync(Guid orderId,
        //    OrderPaymentRequest request)
        //{
        //    var order = await _unitOfWork.Repository<Order, Guid>()
        //        .GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payment, o => o.Table, o => o.OrderItems);
        //    if (order == null)
        //        return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND",
        //            "Order not found.");
        //    _logger.LogInformation($"Initiating payment for Order {orderId} with method {request.PaymentMethod}");
        //    // Simulate payment logic
        //    if (request.PaymentMethod == PaymentMethodEnums.COD)
        //    {
        //        order.PaymentStatus = PaymentStatusEnums.Paid;
        //        order.Status = OrderStatus.Completed; // Update order status to Completed
        //        order.LastUpdatedTime = DateTime.UtcNow;
        //        if (order.Payment != null)
        //        {
        //            order.Payment.PaymentStatus = PaymentStatusEnums.Paid;


        //        }

        //        foreach (OrderItem item in order.OrderItems)
        //        {
        //            item.Status = OrderItemStatus.Completed; // Mark all items as completed
        //            item.LastUpdatedTime = DateTime.UtcNow;
        //        }

        //        // Update table status to Available when payment is completed
        //        if (order.Table != null)
        //        {
        //            order.Table.Status = TableEnums.Available;
        //            await _unitOfWork.Repository<Table, Guid>().UpdateAsync(order.Table);
        //        }

        //        await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
        //        await _unitOfWork.SaveChangesAsync();

        //        // Send payment status notification
        //        if (_notificationService != null)
        //        {
        //            try
        //            {
        //                var paymentNotification = new PaymentStatusNotification
        //                {
        //                    OrderId = orderId,
        //                    TableId = order.TableId ?? Guid.Empty,
        //                    TableName = order.Table?.Name ?? "Unknown",
        //                    OldStatus = PaymentStatusEnums.Pending,
        //                    NewStatus = PaymentStatusEnums.Paid,
        //                    PaymentMethod = request.PaymentMethod,
        //                    TotalAmount = order.TotalPrice,
        //                    UpdatedAt = DateTime.UtcNow
        //                };

        //                await _notificationService.SendPaymentStatusNotificationAsync(paymentNotification);
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, "Failed to send payment status notification");
        //            }
        //        }

        //        return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, "PAID",
        //            new OrderPaymentResponse
        //            {
        //                OrderId = orderId,
        //                PaymentStatus = PaymentStatusEnums.Paid,
        //                Message = "Payment successful (COD)"
        //            });
        //    }
        //    else if (request.PaymentMethod == PaymentMethodEnums.VNPay)
        //    {
        //        // Simulate VNPay payment URL
        //        string paymentUrl = $"https://sandbox.vnpayment.vn/payment/{orderId}";
        //        order.PaymentStatus = PaymentStatusEnums.Pending;
        //        order.LastUpdatedTime = DateTime.UtcNow;
        //        await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
        //        await _unitOfWork.SaveChangesAsync();
        //        return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, "PAYMENT_INITIATED",
        //            new OrderPaymentResponse
        //            {
        //                OrderId = orderId,
        //                PaymentStatus = PaymentStatusEnums.Pending,
        //                PaymentUrl = paymentUrl,
        //                Message = "Redirect to VNPay for payment."
        //            });
        //    }
        //    else
        //    {
        //        return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status400BadRequest,
        //            "UNSUPPORTED_PAYMENT", "Unsupported payment method.");
        //    }
        //}


        public async Task<BaseResponseModel<InforBill>> CreateBill(Guid idOrder)
        {
            // Existing logic retained
            try
            {
                var temp = await _unitOfWork.Repository<Order, Guid>().GetByIdAsync(idOrder);
                decimal sum = 0;
                foreach (var tempOrderItem in temp.OrderItems)
                {
                    sum += tempOrderItem.ProductSize.Price;
                    sum += tempOrderItem.OrderItemTopping.Sum(topping => topping.Price);
                }

                var res = MapOrder.MapBill(temp, sum);
                updateStatus(temp, PaymentStatusEnums.Pending);
                return new BaseResponseModel<InforBill>(StatusCodes.Status200OK, "SUCCESS", res);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<OrderStatus> CalculateOrderStatusAsync(IEnumerable<OrderItem> items, Table table)
        {
            if (items.All(i => i.Status == OrderItemStatus.Completed))
            {
                return OrderStatus.Completed;
            }

            if (items.All(i => i.Status == OrderItemStatus.Cancelled))
            {
              return OrderStatus.Cancelled;
            }

            if (items.Any(i =>
                    i.Status == OrderItemStatus.Ready ||
                    i.Status == OrderItemStatus.Preparing ||
                    i.Status == OrderItemStatus.Served ||
                    i.Status == OrderItemStatus.Remark))
                return OrderStatus.Delivering;

            return OrderStatus.Pending;
        }


        /// <summary>
        /// Tính tổng tiền của order từ danh sách orderItems.
        /// - Bỏ qua món có Status = Cancelled hoặc Returned.
        /// - Tính theo công thức: (Giá size + tổng topping) * số lượng.
        /// </summary>
        private decimal CalculateOrderTotal(IEnumerable<OrderItem> orderItems)
        {
            return (orderItems == null)
                ? 0
                : orderItems
                    .Where(i => i.Status != OrderItemStatus.Cancelled)
                    .Sum(i => (i.ProductSize?.Price ?? 0) +
                              (i.OrderItemTopping?.Sum(t => t.Topping?.Price ?? 0) ?? 0));
        }

        private async Task<Table> ChangeTableToAvailableAsync(Table table)
        {
            if (table == null) return null;

            table.Status = TableEnums.Available;
            table.DeviceId = null;
            table.IsQrLocked = false;
            table.LockedAt = null;
            table.LastAccessedAt = null;
            table.LastUpdatedTime = DateTime.UtcNow;
            table.LastUpdatedBy = "system"; // hoặc userId nếu có context

            await _unitOfWork.Repository<Table, Guid>().UpdateAsync(table);
            return table;
        }




        protected async void updateStatus(Order order, PaymentStatusEnums status)
        {
            // ✅ Cập nhật tất cả Payment của Order sang cùng trạng thái
            if (order.Payments != null && order.Payments.Any())
            {
                foreach (var payment in order.Payments)
                {
                    payment.PaymentStatus = status;
                    payment.LastUpdatedTime = DateTime.UtcNow;
                    _unitOfWork.Repository<Payment, Guid>().Update(payment);
                }
            }
            order.PaymentStatus = status;
            _unitOfWork.Repository<Order, Guid>().Update(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<BaseResponseModel<List<OrderResponse>>> GetALLOrderByIdTabeleWihPending(Guid idTable)
        {
            // var res = await _unitOfWork.Repository<Order,Guid>().GetAllAsync()

            throw new NotImplementedException();
        }

        public async Task<Dictionary<Guid, OrderStaticsResponse>> GetOrderStatsByTableIds(IEnumerable<Guid> tableIds)
        {
            var tableIdsList = tableIds.ToList();
            if (!tableIdsList.Any())
                return new Dictionary<Guid, OrderStaticsResponse>();

            // 🔹 Lấy tất cả orders của các bàn trong 1 query duy nhất
            var allOrders = await _unitOfWork.Repository<Order, Guid>()
                .GetAllWithSpecAsync(new OrdersByTableIdsSpecification(tableIdsList), true);

            // 🔹 Gom nhóm order theo TableId
            var ordersByTableId = allOrders
                .GroupBy(o => o.TableId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new Dictionary<Guid, OrderStaticsResponse>(tableIdsList.Count);

            // 🔹 Xử lý từng bàn
            foreach (var tableId in tableIdsList)
            {
                var orders = ordersByTableId.GetValueOrDefault(tableId, new List<Order>());
                result[tableId] = CalculateOrderStats(orders);
            }

            return result;
        }

        private static OrderStaticsResponse CalculateOrderStats(IEnumerable<Order> orders)
        {
            // 🧩 Bàn chưa có order nào
            if (orders == null || !orders.Any())
            {
                return new OrderStaticsResponse
                {
                    PaymentStatus = PaymentStatusEnums.None,
                    TotalOrderItems = 0,
                    DeliveredCount = 0,
                    ServedCount = 0,
                    PaidCount = 0
                };
            }

            // 🔹 Gom tất cả item của các order
            var allItems = orders
                .Where(o => o.OrderItems != null)
                .SelectMany(o => o.OrderItems.Select(item => new
                {
                    OrderPaymentStatus = o.PaymentStatus,
                    OrderStatus = o.Status,
                    ItemStatus = item.Status
                }))
                .ToList();

            var totalItems = allItems.Count;

            // 🔹 Đếm số món đã thanh toán (Completed + Order đã Paid)
            var paidItems = allItems.Count(x =>
                x.OrderPaymentStatus == PaymentStatusEnums.Paid &&
                x.ItemStatus == OrderItemStatus.Completed);

            // 🔹 Xác định trạng thái tổng hợp của các order
            bool allCancelledOrders = orders.All(o => o.Status == OrderStatus.Cancelled);
            bool allCompletedOrders = orders.All(o => o.Status == OrderStatus.Completed);
            bool hasActiveOrder = orders.Any(o =>
                o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

            PaymentStatusEnums finalPaymentStatus;

            // ✅ 1️⃣ Nếu toàn bộ order bị huỷ → bàn không còn thanh toán nào
            if (allCancelledOrders)
            {
                finalPaymentStatus = PaymentStatusEnums.None;
            }
            // ✅ 2️⃣ Nếu toàn bộ order đã hoàn tất → đã thanh toán
            else if (allCompletedOrders)
            {
                finalPaymentStatus = PaymentStatusEnums.Paid;
            }
            // ✅ 3️⃣ Nếu bàn còn order đang hoạt động
            else if (hasActiveOrder)
            {
                var currentOrder = orders
                    .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                    .OrderByDescending(o => o.CreatedTime) // nếu có thuộc tính CreatedOn
                    .FirstOrDefault();

                // Nếu không có CreatedOn thì dùng ID lớn nhất (hoặc logic khác)
                finalPaymentStatus = currentOrder?.PaymentStatus ?? PaymentStatusEnums.None;
            }
            // ✅ 4️⃣ Nếu không còn order hoạt động (tức tất cả done hoặc cancel)
            else
            {
                finalPaymentStatus = PaymentStatusEnums.None;
            }

            // 🔹 Trả về kết quả thống kê
            return new OrderStaticsResponse
            {
                PaymentStatus = finalPaymentStatus,
                TotalOrderItems = totalItems,
                DeliveredCount = allItems.Count(x =>
                    x.ItemStatus is OrderItemStatus.Ready or OrderItemStatus.Served or OrderItemStatus.Remark or OrderItemStatus.Completed),
                ServedCount = allItems.Count(x =>
                    x.ItemStatus is OrderItemStatus.Served or OrderItemStatus.Completed),
                PaidCount = paidItems
            };
        }


        // public async Task<BaseResponseModel<List<OrderResponse>>> GetOrderByDeviceToken(string idTable, string token)
        // {
        //     // var temp = await _unitOfWork.Repository<Order, Guid>()
        //     //     .GetAllWithSpecWithInclueAsync(new OrderWithDetailsSpecification(token), true);
        //
        //     var listas = await _unitOfWork.Repository<Order, Guid>()
        //         .GetAllWithSpecWithInclueAsync(new OrderWithDetailsSpecification(token, idTable),
        //             true);
        //
        //     
        //     var response = _mapper.Map<List<OrderResponse>>(listas);
        //     return new BaseResponseModel<List<OrderResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        // }


        public async Task<BaseResponseModel<OrderResponse>> GetOrderByDeviceToken(string idTable, string token)
        {
            // var temp = await _unitOfWork.Repository<Order, Guid>()
            //     .GetAllWithSpecWithInclueAsync(new OrderWithDetailsSpecification(token), true);

            var listas = await _unitOfWork.Repository<Order, Guid>()
                .GetAllWithSpecWithInclueAsync(new OrderWithDetailsSpecification(token, idTable),
                    true);

            // Lấy order mới nhất theo CreatedTime
            var latestOrder = listas
                .OrderByDescending(o => o.CreatedTime)
                .FirstOrDefault();
            if (latestOrder == null)
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status404NotFound, "NOT_FOUND",
                    "Order not found.");
            var response = _mapper.Map<OrderResponse>(latestOrder);
            return new BaseResponseModel<OrderResponse>(StatusCodes.Status200OK, "SUCCESS", response);
        }


        #region order item   improved 
        public async Task<BaseResponseModel<OrderResponse>> CreateOrderAsyncs(CreateOrderRequest request)
        {
            if (!HasValidItems(request))
                return BadRequest("NO_ITEMS", "Order must have at least one item.");

            
            var table = await GetAndValidateTableAsync(request.TableId);
            await EnsureNoOrderFromDeviceAsync(request.TableId, request.deviceToken);

            table.Status = TableEnums.Occupied;
            _unitOfWork.Repository<Table, Guid>().Update(table);

            var order = InitializeOrder(request);
            decimal currentTotal = 0m; // 🔹 khai báo biến này

            var lookupData = await LoadProductDataAsync(request.Items);

            foreach (var itemReq in request.Items)
            {
                var orderitem = await BuildOrderItemAsync(itemReq, lookupData, currentTotal);
                currentTotal = orderitem.total;
                order.OrderItems.Add(orderitem.orderItem);

            }

            order.TotalPrice = currentTotal;
            await _unitOfWork.Repository<Order, Guid>().AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            
            var response = _mapper.Map<OrderResponse>(order);
            return new BaseResponseModel<OrderResponse>(StatusCodes.Status201Created, "ORDER_CREATED", response);
        }

        public async Task<BaseResponseModel<OrderResponse>> HandleOrderAsyncs(CreateOrderRequest request)
        {
            if (!HasValidItems(request))
                return BadRequest("NO_ITEMS", "Order must have at least one item.");


            var existingOrder = await _unitOfWork.Repository<Order, Guid>()
                .GetWithSpecAsync(new OrderSpecification(request.TableId), false);

            if (existingOrder != null)
            {
                if (!existingOrder.LastUpdatedBy.Equals(request.deviceToken))
                    return BadRequest("INVALID_DEVICE", "Thiết bị không có quyền đặt hàng");

                if (existingOrder.Table != null && existingOrder.Table.Status != TableEnums.Occupied)
                {
                    existingOrder.Table.Status = TableEnums.Occupied;
                }


                decimal currentTotal = 0m;
                var lookupData = await LoadProductDataAsync(request.Items);

                foreach (var itemReq in request.Items)
                {
                    var orderItem = await BuildOrderItemAsync(itemReq, lookupData, currentTotal);
                    existingOrder.OrderItems.Add(orderItem.orderItem);
                }
                existingOrder.TotalPrice += currentTotal;
                existingOrder.LastUpdatedTime = DateTime.UtcNow;

                _unitOfWork.Repository<Order, Guid>().Update(existingOrder);
                await _unitOfWork.SaveChangesAsync();
               

                var response = _mapper.Map<OrderResponse>(existingOrder);
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status200OK, "ORDER_UPDATED", response);
            }

            return await CreateOrderAsync(request);
        }
        #endregion

        #region Helper Methods

        private bool HasValidItems(CreateOrderRequest request) =>
            request.Items != null && request.Items.Any();

        private BaseResponseModel<OrderResponse> BadRequest(string code, string message) =>
            new BaseResponseModel<OrderResponse>(StatusCodes.Status400BadRequest, code, message);

        private async Task<Table> GetAndValidateTableAsync(Guid tableId)
        {
            var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(tableId);
            if (table == null)
                throw new ErrorException(StatusCodes.Status400BadRequest, "TABLE_NOT_FOUND", "Table not found.");

            if (!table.Status.Equals(TableEnums.Available))
                throw new ErrorException(StatusCodes.Status409Conflict, "TABLE_NOT_AVAILABLE", "Bàn này đang được sử dụng.");

            return table;
        }

        private async Task EnsureNoOrderFromDeviceAsync(Guid tableId, string deviceToken)
        {
            var hasOrderFromDevice = await _unitOfWork.Repository<Order, Guid>()
                .AnyAsync(o => o.TableId == tableId && o.CreatedBy == deviceToken);

            if (hasOrderFromDevice)
                throw new ErrorException(StatusCodes.Status409Conflict, "DEVICE_ALREADY_ORDERED", "Thiết bị này đã đặt hàng.");
        }

        private Order InitializeOrder(CreateOrderRequest request)
        {
            var now = DateTime.UtcNow;
            return new Order
            {
                Id = Guid.NewGuid(),
                TableId = request.TableId,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatusEnums.Pending,
                CreatedBy = request.deviceToken,
                LastUpdatedBy = request.deviceToken,
                CreatedTime = now,
                LastUpdatedTime = now,
                OrderItems = new List<OrderItem>()
            };
        }

        private async Task<(Dictionary<Guid, Product> products, Dictionary<Guid, ProductSize> sizes, Dictionary<Guid, Topping> toppings)>
            LoadProductDataAsync(IEnumerable<CreateOrderItemRequest> items)
        {
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            var sizeIds = items.Select(i => i.ProductSizeId).Distinct().ToList();
            var toppingIds = items.SelectMany(i => i.ToppingIds).Distinct().ToList();

            var products = (await _unitOfWork.Repository<Product, Guid>().GetListAsync(p => productIds.Contains(p.Id)))
                .ToDictionary(p => p.Id);
            var sizes = (await _unitOfWork.Repository<ProductSize, Guid>().GetListAsync(s => sizeIds.Contains(s.Id)))
                .ToDictionary(s => s.Id);
            var toppings = (await _unitOfWork.Repository<Topping, Guid>().GetListAsync(t => toppingIds.Contains(t.Id)))
                .ToDictionary(t => t.Id);

            return (products, sizes, toppings);
        }

        private async Task<(OrderItem orderItem, decimal total)> BuildOrderItemAsync(
          CreateOrderItemRequest itemReq,
          (Dictionary<Guid, Product> products, Dictionary<Guid, ProductSize> sizes, Dictionary<Guid, Topping> toppings) lookupData,
          decimal currentTotal)
        {
            if (!lookupData.products.TryGetValue(itemReq.ProductId, out var product) ||
                !lookupData.sizes.TryGetValue(itemReq.ProductSizeId, out var productSize))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, "INVALID_PRODUCT_OR_SIZE", "Invalid product or size.");
            }

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = itemReq.ProductId,
                Product = product,
                ProductSizeId = itemReq.ProductSizeId,
                ProductSize = productSize,
                Note = itemReq.Note,
                Status = OrderItemStatus.Pending,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                OrderItemTopping = new List<OrderItemTopping>()
            };

            currentTotal += productSize.Price;

            foreach (var toppingId in itemReq.ToppingIds)
            {
                if (!lookupData.toppings.TryGetValue(toppingId, out var topping))
                    throw new ErrorException(StatusCodes.Status400BadRequest, "TOPPING_NOT_FOUND", "Topping not found.");

                orderItem.OrderItemTopping.Add(new OrderItemTopping
                {
                    Id = Guid.NewGuid(),
                    ToppingId = topping.Id,
                    Price = topping.Price,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                });

                currentTotal += topping.Price;
            }

            return (orderItem, currentTotal);
        }

       

        #endregion



    }
}