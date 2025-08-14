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

        /// <summary>
        /// Create VNPay payment URL for an order.
        /// </summary>
        /// <remarks>
        /// Initiates VNPay payment for a specific order and returns a redirect URL.
        ///
        /// Sample request:
        /// POST /api/Payment/create-url/{orderId}
        /// {
        ///   "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "moneyUnit": "VND",
        ///   "paymentContent": "Thanh toan don 123"
        /// }
        ///
        /// Notes:
        /// - Client does NOT need to send total amount. It is derived from the order on the server.
        /// - The generated URL is valid for a limited time (configured by `vnp_ExpireDate`).
        /// </remarks>
        /// <param name="orderId">Order identifier</param>
        /// <param name="request">Payment creation parameters (money unit, content)</param>
        /// <returns>Payment URL and pending payment status</returns>
        /// <response code="200">Payment URL created successfully</response>
        /// <response code="404">Order not found</response>
        /// <response code="400">Invalid request</response>
        [HttpPost("create-url/{orderId}")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        public async Task<ActionResult<BaseResponseModel>> CreatePaymentUrl(Guid orderId, [FromBody] PaymentCreateRequest request)
        {
            // Total amount is determined server-side from order.TotalPrice; client should not send it.
            var result = await _paymentService.CreateVNPayPaymentUrl(orderId, request.MoneyUnit, request.PaymentContent);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// VNPay return (client redirect) handler.
        /// </summary>
        /// <remarks>
        /// VNPay redirects the user to this endpoint after payment. This validates the
        /// response and updates the order/payment status accordingly.
        ///
        /// Sample request:
        /// GET /api/Payment/vnpay-return?vnp_TxnRef=...&vnp_ResponseCode=...
        /// </remarks>
        /// <returns>Payment processing result</returns>
        /// <response code="200">Processed VNPay return</response>
        /// <response code="400">Invalid signature</response>
        [HttpGet("vnpay-return")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        public async Task<ActionResult<BaseResponseModel>> VNPayReturn()
        {
            var result = await _paymentService.HandleVNPayReturn(Request.Query);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// VNPay IPN (server-to-server) handler.
        /// </summary>
        /// <remarks>
        /// VNPay calls this endpoint asynchronously to confirm payment status.
        /// This should be used to ensure final status even if the user closes the browser.
        ///
        /// Sample request:
        /// GET /api/Payment/vnpay-ipn?vnp_TxnRef=...&vnp_TransactionStatus=...
        /// </remarks>
        /// <returns>Payment processing result</returns>
        /// <response code="200">Processed VNPay IPN</response>
        /// <response code="400">Invalid signature</response>
        [HttpGet("vnpay-ipn")]
        [ProducesResponseType(typeof(BaseResponseModel<OrderPaymentResponse>), 200)]
        public async Task<ActionResult<BaseResponseModel>> VNPayIpn()
        {
            var result = await _paymentService.HandleVNPayIpn(Request.Query);
            return StatusCode(result.StatusCode, result);
        }
    }
}
