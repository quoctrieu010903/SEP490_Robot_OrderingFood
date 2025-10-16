using Net.payOS.Types;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface;

public interface IPayOSService
{
    Task<BaseResponseModel<OrderPaymentResponse>> CreatePaymentLink(Guid orderId);
    Task HandleWebhook(WebhookType body);
}