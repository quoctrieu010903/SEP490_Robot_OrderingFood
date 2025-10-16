using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayOSController : ControllerBase
    {
        private readonly IPayOSService _payOSService;

        public PayOSController(IPayOSService payOSService)
        {
            _payOSService = payOSService;
        }

        // FE gọi để lấy URL thanh toán
        [HttpPost("create-link/{orderId}")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        public async Task<ActionResult<BaseResponseModel>> CreateLink(Guid orderId)
        {
            var result = await _payOSService.CreatePaymentLink(orderId);
            return StatusCode(result.StatusCode, result);
        }

        // Webhook PayOS
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] WebhookType body)
        {
            await _payOSService.HandleWebhook(body);
            return Ok();
        }
    }
}