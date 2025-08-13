using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-url/{orderId}")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        public async Task<ActionResult<BaseResponseModel>> CreatePaymentUrl(Guid orderId, [FromBody] PaymentCreateRequest request)
        {
            var result = await _paymentService.CreateVNPayPaymentUrl(orderId, request.MoneyUnit, request.PaymentContent);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("vnpay-return")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        public async Task<ActionResult<BaseResponseModel>> VNPayReturn()
        {
            var result = await _paymentService.HandleVNPayReturn(Request.Query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("vnpay-ipn")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        public async Task<ActionResult<BaseResponseModel>> VNPayIpn()
        {
            var result = await _paymentService.HandleVNPayIpn(Request.Query);
            return StatusCode(result.StatusCode, result);
        }
    }
}
