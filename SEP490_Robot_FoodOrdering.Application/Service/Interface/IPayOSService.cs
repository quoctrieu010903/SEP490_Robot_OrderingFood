using Net.payOS.Types;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface;

public interface IPayOSService
{
    Task<BaseResponseModel<OrderPaymentResponse>> CreatePaymentLink(Guid orderId, bool isCustomer);
    Task HandleWebhook(WebhookType body);
    Task<BaseResponseModel<OrderPaymentResponse>> SyncOrderPaymentStatus(Guid orderId);
}