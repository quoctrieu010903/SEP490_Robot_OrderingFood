using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Application.Mapping;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BaseResponseModel<OrderResponse>> CreateOrderAsync(CreateOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status400BadRequest, "NO_ITEMS", "Order must have at least one item.");

            var order = _mapper.Map<Order>(request);
            order.Status = OrderStatus.Pending;
            order.PaymentStatus = PaymentStatusEnums.Pending;
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
                for (int i = 0; i < itemReq.Quantity; i++)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = itemReq.ProductId,
                        Product = product,
                        ProductSizeId = itemReq.ProductSizeId,
                        ProductSize = productSize,
                        Status = OrderItemStatus.Pending,
                        CreatedTime = DateTime.UtcNow,
                        LastUpdatedTime = DateTime.UtcNow
                    };
                    order.OrderItems.Add(orderItem);
                    total += productSize.Price;
                }
            }
            order.TotalPrice = total;
            await _unitOfWork.Repository<Order, bool>().AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            var response = _mapper.Map<OrderResponse>(order);
            return new BaseResponseModel<OrderResponse>(StatusCodes.Status201Created, "ORDER_CREATED", response);
        }

        public async Task<BaseResponseModel<OrderResponse>> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _unitOfWork.Repository<Order, Guid>().GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.OrderItems, o => o.Table, o => o.Payment);
            if (order == null)
                return new BaseResponseModel<OrderResponse>(StatusCodes.Status404NotFound, "NOT_FOUND", "Order not found.");
            var response = _mapper.Map<OrderResponse>(order);
            return new BaseResponseModel<OrderResponse>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<List<OrderResponse>>> GetOrdersAsync()
        {
            var orders = await _unitOfWork.Repository<Order, Order>().GetAllWithIncludeAsync(true, o => o.OrderItems, o => o.Table, o => o.Payment);
            var response = _mapper.Map<List<OrderResponse>>(orders.ToList());
            return new BaseResponseModel<List<OrderResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<OrderItemResponse>> UpdateOrderItemStatusAsync(Guid orderId, Guid orderItemId, UpdateOrderItemStatusRequest request)
        {
            var order = await _unitOfWork.Repository<Order, Guid>().GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.OrderItems);
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
            await _unitOfWork.Repository<Order, Order>().UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            var response = _mapper.Map<OrderItemResponse>(item);
            return new BaseResponseModel<OrderItemResponse>(StatusCodes.Status200OK, "ITEM_STATUS_UPDATED", response);
        }

        public async Task<BaseResponseModel<List<OrderItemResponse>>> GetOrderItemsAsync(Guid orderId)
        {
            var order = await _unitOfWork.Repository<Order, Guid>().GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.OrderItems);
            if (order == null)
                return new BaseResponseModel<List<OrderItemResponse>>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found.");
            var response = _mapper.Map<List<OrderItemResponse>>(order.OrderItems.ToList());
            return new BaseResponseModel<List<OrderItemResponse>>(StatusCodes.Status200OK, "SUCCESS", response);
        }

        public async Task<BaseResponseModel<OrderPaymentResponse>> InitiatePaymentAsync(Guid orderId, OrderPaymentRequest request)
        {
            var order = await _unitOfWork.Repository<Order, Guid>().GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payment);
            if (order == null)
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found.");
            _logger.LogInformation($"Initiating payment for Order {orderId} with method {request.PaymentMethod}");
            // Simulate payment logic
            if (request.PaymentMethod == PaymentMethodEnums.COD)
            {
                order.PaymentStatus = PaymentStatusEnums.Paid;
                if (order.Payment != null)
                {
                    order.Payment.PaymentStatus = PaymentStatusEnums.Paid;
                }
                await _unitOfWork.Repository<Order, Order>().UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, "PAID", new OrderPaymentResponse { OrderId = orderId, PaymentStatus = PaymentStatusEnums.Paid, Message = "Payment successful (COD)" });
            }
            else if (request.PaymentMethod == PaymentMethodEnums.VNPay)
            {
                // Simulate VNPay payment URL
                string paymentUrl = $"https://sandbox.vnpayment.vn/payment/{orderId}";
                order.PaymentStatus = PaymentStatusEnums.Pending;
                await _unitOfWork.Repository<Order, Order>().UpdateAsync(order);
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

        protected async void updateStatus(Order order, PaymentStatusEnums status)
        {
            if (order.Payment != null)
            {
                order.Payment.PaymentStatus = status;
            }
            order.PaymentStatus = status;
            _unitOfWork.Repository<Order, Guid>().Update(order);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}