using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IOrderService
    {
        Task<BaseResponseModel<OrderResponse>> CreateOrderAsync(CreateOrderRequest request);
        Task<BaseResponseModel<OrderResponse>> GetOrderByIdAsync(Guid orderId);
        Task<BaseResponseModel<List<OrderResponse>>> GetOrdersAsync();
        Task<BaseResponseModel<OrderItemResponse>> UpdateOrderItemStatusAsync(Guid orderId, Guid orderItemId, UpdateOrderItemStatusRequest request);
        Task<BaseResponseModel<List<OrderItemResponse>>> GetOrderItemsAsync(Guid orderId);
        Task<BaseResponseModel<OrderPaymentResponse>> InitiatePaymentAsync(Guid orderId, OrderPaymentRequest request);
        Task<BaseResponseModel<InforBill>> CreateBill(Guid idOrder);
    }
}