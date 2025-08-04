using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Enums;


namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IOrderService
    {
        Task<BaseResponseModel<OrderResponse>> CreateOrderAsync(CreateOrderRequest request);
        Task<BaseResponseModel<OrderResponse>> HandleOrderAsync(CreateOrderRequest request);
        Task<BaseResponseModel<OrderResponse>> GetOrderByIdAsync(Guid orderId);
        Task<PaginatedList<OrderResponse>> GetOrdersAsync(PagingRequestModel paging, string? ProductName);

        Task<BaseResponseModel<OrderItemResponse>> UpdateOrderItemStatusAsync(Guid orderId, Guid orderItemId,
            UpdateOrderItemStatusRequest request);

        Task<BaseResponseModel<List<OrderItemResponse>>> GetOrderItemsAsync(Guid orderId);
        Task<BaseResponseModel<OrderPaymentResponse>> InitiatePaymentAsync(Guid orderId, OrderPaymentRequest request);
        Task<BaseResponseModel<InforBill>> CreateBill(Guid idOrder);
        Task<BaseResponseModel<List<OrderResponse>>> GetOrdersbyTableiDAsync(Guid Orderid, Guid TableId);
        Task<BaseResponseModel<List<OrderResponse>>> GetOrdersByTableIdOnlyAsync(Guid tableId);

        Task<BaseResponseModel<List<OrderResponse>>>
            GetOrdersByTableIdWithStatusAsync(Guid tableId, OrderStatus status);

        Task<Dictionary<Guid, OrderStaticsResponse>> GetOrderStatsByTableIds(IEnumerable<Guid> tableIds);
    }
}