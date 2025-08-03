using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
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


namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IOrderItemReposotory _orderItemReposotory;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper,IOrderItemReposotory orderItemReposotory, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _orderItemReposotory = orderItemReposotory;
        }

        public async Task<BaseResponseModel<OrderResponse>> CreateOrderAsync(CreateOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status400BadRequest, "NO_ITEMS", "Order must have at least one item.");

            var order = _mapper.Map<Order>(request);
           
            order.Status = OrderStatus.Pending;
            order.PaymentStatus = PaymentStatusEnums.Pending;
            
            // Load the table entity to avoid null reference exception
            var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(request.TableId);
            if (table == null)
                throw new ErrorException(StatusCodes.Status400BadRequest, "TABLE_NOT_FOUND", "Table not found.");
            
            table.Status = TableEnums.Occupied;
            _unitOfWork.Repository<Table, Guid>().Update(table);
            
            order.CreatedTime = DateTime.UtcNow;
            order.LastUpdatedTime = DateTime.UtcNow;
            order.OrderItems = new List<OrderItem>();
            decimal total = 0;
            foreach (var itemReq in request.Items)
            {
                var product = await _unitOfWork.Repository<Product, Guid>().GetByIdAsync(itemReq.ProductId);
                var productSize = await _unitOfWork.Repository<ProductSize, Guid>().GetByIdAsync(itemReq.ProductSizeId);
                if (product == null || productSize == null)
                   throw new ErrorException(StatusCodes.Status400BadRequest, "INVALID_PRODUCT_OR_SIZE", "Invalid product or size.");
               
                    var orderItem = new OrderItem
                    {
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
                    order.OrderItems.Add(orderItem);
                     
                    total += productSize.Price;

                foreach (var toppingId in itemReq.ToppingIds)
                {
                    var productTopping = await _unitOfWork.Repository<ProductTopping, Guid>()
                        .GetWithSpecAsync(new BaseSpecification<ProductTopping>(pt => pt.ProductId == itemReq.ProductId && pt.ToppingId == toppingId), true);

                    if (productTopping == null)
                        throw new ErrorException(StatusCodes.Status400BadRequest, "INVALID_TOPPING", $"Topping {toppingId} is not valid for Product {itemReq.ProductId}");

                    var topping = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(toppingId);
                    if (topping == null)
                        throw new ErrorException(StatusCodes.Status400BadRequest, "TOPPING_NOT_FOUND", "Topping not found.");

                    orderItem.OrderItemTopping.Add(new OrderItemTopping
                    {
                        ToppingId = topping.Id,
                        Price = topping.Price,
                        CreatedTime = DateTime.UtcNow,
                        LastUpdatedTime = DateTime.UtcNow
                    });

                    total += topping.Price;
                }
            }
            order.TotalPrice = total;
            await _unitOfWork.Repository<Order, bool>().AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            var response = _mapper.Map<OrderResponse>(order);
            return new BaseResponseModel<OrderResponse>(StatusCodes.Status201Created, "ORDER_CREATED", response);
        }

        public async Task<BaseResponseModel<OrderResponse>> HandleOrderAsync(CreateOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status400BadRequest, "NO_ITEMS", "Order must have at least one item.");

            // 1. Tìm order đang pending của bàn này
            var existingOrder = await _unitOfWork.Repository<Order, Guid>()
                .GetWithSpecAsync( new OrderSpecification(request.TableId), true);

            // 2. Nếu có order -> thêm item vào và cập nhật lại giá
            if (existingOrder != null)
            {
                // Ensure table is marked as occupied when adding items to existing order
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
                    var productSize = await _unitOfWork.Repository<ProductSize, Guid>().GetByIdAsync(itemReq.ProductSizeId);
                    if (product == null || productSize == null)
                        throw new ErrorException(StatusCodes.Status400BadRequest, "INVALID_PRODUCT_OR_SIZE", "Invalid product or size.");

                    //for (int i = 0; i < itemReq.Quantity; i++)
                    //{
                        var orderItem = new OrderItem
                        {
                            OrderId = existingOrder.Id,
                            ProductId = itemReq.ProductId,
                            ProductSizeId = itemReq.ProductSizeId,
                            Note = itemReq.Note,
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
                            .GetWithSpecAsync(new BaseSpecification<ProductTopping>(pt => pt.ProductId == itemReq.ProductId && pt.ToppingId == toppingId),true);

                        if (productTopping == null)
                            throw new ErrorException(StatusCodes.Status400BadRequest, "INVALID_TOPPING", $"Topping {toppingId} is not valid for Product {itemReq.ProductId}");

                        var topping = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(toppingId);
                        if (topping == null)
                            throw new ErrorException(StatusCodes.Status400BadRequest, "TOPPING_NOT_FOUND", "Topping not found.");

                        orderItem.OrderItemTopping.Add(new OrderItemTopping
                        {
                            ToppingId = topping.Id,
                            Price = topping.Price,
                            CreatedTime = DateTime.UtcNow,
                            LastUpdatedTime = DateTime.UtcNow
                        });

                        addedTotal += topping.Price;
                    }
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


        public async Task<BaseResponseModel<OrderResponse>> GetOrderByIdAsync(Guid orderId
            )
        {
            var order = await _unitOfWork.Repository<Order, Guid>().GetWithSpecAsync(new OrderSpecification(orderId, true),  true);
            if (order == null)
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status404NotFound, "NOT_FOUND", "Order not found.");
            var response = _mapper.Map<OrderResponse>(order);
            return new BaseResponseModel<OrderResponse>(StatusCodes.Status200OK, "SUCCESS", response);
        }
        public async Task<PaginatedList<OrderResponse>> GetOrdersAsync(PagingRequestModel paging , string? ProductName)
        {
            // Tính start và end theo múi giờ Việt Nam
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var todayVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).Date;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayVN, vnTimeZone);
            var endUtc = startUtc.AddDays(1);
            var specification = new OrderSpecification(ProductName, startUtc, endUtc);
          
            var orders = await _unitOfWork.Repository<Order, Order>().GetAllWithSpecAsync( specification, true);
            var response = _mapper.Map<List<OrderResponse>>(orders);


            // Group OrderItems by ProductName or Status for the UI grouping
            foreach (var order in response)
            {
                order.Items = order.Items
                    .GroupBy(item => new { item.ProductName, item.Status }) // Group by ProductName and Status
                    .Select(g => new OrderItemResponse
                    {
                        Id = g.First().Id,
                        ProductId = g.First().ProductId,
                        ProductName = g.Key.ProductName,
                        ProductSizeId = g.First().ProductSizeId,
                        SizeName = g.First().SizeName,
                        Quantity = g.Sum(x => 1), // Count items in the group
                        Price = g.First().Price * g.Count(), // Total price for the group
                        Status = g.Key.Status,
                        ImageUrl = g.First().ImageUrl,
                        CreatedTime = g.Min(x => x.CreatedTime),
                        Toppings = g.SelectMany(x => x.Toppings).Distinct().ToList() // Combine toppings
                    }).ToList();
            }

            return PaginatedList<OrderResponse>.Create(response, paging.PageNumber, paging.PageSize);
        }

        //public async Task<PaginatedList<OrderResponse>> GetOrdersAsync(PagingRequestModel paging)
        //{
        //    var orders = await _unitOfWork.Repository<Order, Order>().GetAllWithSpecAsync( new OrderSpecification(), true);
        //    var response = _mapper.Map<List<OrderResponse>>(orders);
        //    return  PaginatedList<OrderResponse>.Create(response, paging.PageNumber, paging.PageSize);      
        //}
        public async Task<BaseResponseModel<List<OrderResponse>>> GetOrdersbyTableiDAsync(Guid Orderid,Guid TableId)
        {
            var orders = await _unitOfWork.Repository<Order, Order>().GetAllWithSpecAsync(new OrderSpecification(Orderid, TableId, true), true);
            var response = _mapper.Map<List<OrderResponse>>(orders);
            return new BaseResponseModel<List<OrderResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<List<OrderResponse>>> GetOrdersByTableIdOnlyAsync(Guid tableId)
        {
            var orders = await _unitOfWork.Repository<Order, Order>().GetAllWithSpecAsync(new OrderSpecification(true, tableId), true);
            var response = _mapper.Map<List<OrderResponse>>(orders);
            return new BaseResponseModel<List<OrderResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<List<OrderResponse>>> GetOrdersByTableIdWithStatusAsync(Guid tableId, OrderStatus status)
        {
            var orders = await _unitOfWork.Repository<Order, Order>().GetAllWithSpecAsync(new OrderSpecification(tableId, status), true);
            
            // Debug: Log the orders to see what data is being returned
            foreach (var order in orders)
            {
                _logger.LogInformation($"Order {order.Id} has {order.OrderItems?.Count ?? 0} items");
                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        _logger.LogInformation($"OrderItem {item.Id}: ProductSize={item.ProductSize?.Id}, Price={item.ProductSize?.Price}, Product={item.Product?.Name}");
                    }
                }
            }
            
            var response = _mapper.Map<List<OrderResponse>>(orders);
            return new BaseResponseModel<List<OrderResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<OrderItemResponse>> UpdateOrderItemStatusAsync(Guid orderId, Guid orderItemId, UpdateOrderItemStatusRequest request)
        {
            var order = await _unitOfWork.Repository<Order, Guid>().GetWithSpecAsync( new OrderSpecification(orderId,true), true );
            if (order == null)
                return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found.");
            var item = order.OrderItems.FirstOrDefault(i => i.Id == orderItemId);
            if (item == null)
                return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status404NotFound, "ITEM_NOT_FOUND", "Order item not found.");
            var oldStatus = item.Status;
            item.Status = request.Status;
            item.LastUpdatedTime = DateTime.UtcNow;
            _logger.LogInformation($"OrderItem {item.Id} status changed from {oldStatus} to {item.Status} in Order {orderId}");
            // Update order status automatically
            var oldOrderStatus = order.Status;
            order.Status = CalculateOrderStatus(order.OrderItems);
            order.LastUpdatedTime = DateTime.UtcNow;
            if (oldOrderStatus != order.Status)
            {
                _logger.LogInformation($"Order {orderId} status changed from {oldOrderStatus} to {order.Status}");
            }
            await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            var response = _mapper.Map<OrderItemResponse>(item);
            return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status200OK, "ITEM_STATUS_UPDATED", response);
        }

        public async Task<BaseResponseModel<List<OrderItemResponse>>> GetOrderItemsAsync(Guid orderId)
        {
            var order = await _unitOfWork.Repository<Order, Guid>().GetWithSpecAsync(new OrderSpecification(orderId, true),  true);
            if (order == null)
                return new BaseResponseModel<List<OrderItemResponse>>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found.");
            var response = _mapper.Map<List<OrderItemResponse>>(order.OrderItems.ToList());
            return new BaseResponseModel<List<OrderItemResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<OrderPaymentResponse>> InitiatePaymentAsync(Guid orderId, OrderPaymentRequest request)
        {
            var order = await _unitOfWork.Repository<Order, Guid>().GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payment, o => o.Table);
            if (order == null)
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found.");
            _logger.LogInformation($"Initiating payment for Order {orderId} with method {request.PaymentMethod}");
            // Simulate payment logic
            if (request.PaymentMethod == PaymentMethodEnums.COD)
            {
                order.PaymentStatus = PaymentStatusEnums.Paid;
                order.Status = OrderStatus.Completed; // Update order status to Completed
                order.LastUpdatedTime = DateTime.UtcNow;
                if (order.Payment != null)
                {
                    order.Payment.PaymentStatus = PaymentStatusEnums.Paid;
                }
                // Update table status to Available when payment is completed
                if (order.Table != null)
                {
                    order.Table.Status = TableEnums.Available;
                    await _unitOfWork.Repository<Table, Guid>().UpdateAsync(order.Table);
                }
                await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, "PAID", new OrderPaymentResponse { OrderId = orderId, PaymentStatus = PaymentStatusEnums.Paid, Message = "Payment successful (COD)" });
            }
            else if (request.PaymentMethod == PaymentMethodEnums.VNPay)
            {
                // Simulate VNPay payment URL
                string paymentUrl = $"https://sandbox.vnpayment.vn/payment/{orderId}";
                order.PaymentStatus = PaymentStatusEnums.Pending;
                order.LastUpdatedTime = DateTime.UtcNow;
                await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, "PAYMENT_INITIATED", new OrderPaymentResponse { OrderId = orderId, PaymentStatus = PaymentStatusEnums.Pending, PaymentUrl = paymentUrl, Message = "Redirect to VNPay for payment." });
            }
            else
            {
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status400BadRequest, "UNSUPPORTED_PAYMENT", "Unsupported payment method.");
            }
        }
        


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

        private OrderStatus CalculateOrderStatus(IEnumerable<OrderItem> items)
        {
            if (items.All(i => i.Status == OrderItemStatus.Completed))
                return OrderStatus.Completed;
            if (items.All(i => i.Status == OrderItemStatus.Cancelled))
                return OrderStatus.Cancelled;
            if (items.Any(i => i.Status == OrderItemStatus.Ready || i.Status == OrderItemStatus.Preparing || i.Status == OrderItemStatus.Served))
                return OrderStatus.Delivering;
            return OrderStatus.Pending;
        }

        protected async void updateStatus(Order order, PaymentStatusEnums status )
        {
            if (order.Payment != null)
            {
                order.Payment.PaymentStatus = status;
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

        public async Task<OrderStaticsResponse> GetOrderStatsByTableId(Guid tableId)
        {
            var orders = await _unitOfWork.Repository<Order, Guid>().GetAllWithSpecAsync(new OrderSpecification(tableId), true);

           
            int deliveredCount = 0;
            int paidCount = 0;
            int totalOrderItems = 0;

            foreach (var order in orders)
            {
                if (order.OrderItems == null)
                    continue;

                foreach (var item in order.OrderItems)
                {
                    totalOrderItems++;

                    if (item.Status == OrderItemStatus.Served)
                        deliveredCount++;
                }

                if (order.PaymentStatus == PaymentStatusEnums.Paid) // hoặc order.IsPaid == true tùy theo field bạn có
                    paidCount++;
            }
            
            return new OrderStaticsResponse
            {
                DeliveredCount = deliveredCount,
                TotalOrderItems = totalOrderItems,
                PaidCount = paidCount,
               
               
            };
        }
    }
}