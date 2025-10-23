using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Net.payOS.Types;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using System;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayOSController : ControllerBase
    {
        private readonly IPayOSService _payOSService;
        private readonly ILogger<PayOSController> _logger;

        public PayOSController(IPayOSService payOSService, ILogger<PayOSController> logger)
        {
            _payOSService = payOSService;
            _logger = logger;
        }

        // FE gọi để lấy URL thanh toán
        [HttpPost("create-link/{orderId}")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        public async Task<ActionResult<BaseResponseModel>> CreateLink(Guid orderId, bool isCustomer)
        {
            var result = await _payOSService.CreatePaymentLink(orderId,  isCustomer);
            return StatusCode(result.StatusCode, result);
        }

        // Webhook PayOS
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] WebhookType body)
        {
            _logger.LogInformation(
                "PayOS webhook hit: code={Code}, success={Success}, orderCode={OrderCode}",
                body.code,
                body.success,
                body.data != null ? body.data.orderCode : null
            );

            // also dump compact JSON for diagnostic
            try
            {
                _logger.LogInformation("PayOS webhook raw body: {Body}", JsonSerializer.Serialize(body));
            }
            catch { /* ignore serialization issues */ }

            try
            {
                await _payOSService.HandleWebhook(body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayOS webhook handling failed");
                // Return 200 so PayOS doesn't retry aggressively while we investigate
            }

            return Ok();
        }

        // Manual sync to fix orders if webhook missed
        [HttpPost("sync/{orderId}")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        public async Task<ActionResult<BaseResponseModel>> Sync(Guid orderId)
        {
            var result = await _payOSService.SyncOrderPaymentStatus(orderId);
            return StatusCode(result.StatusCode, result);
        }
    }
}