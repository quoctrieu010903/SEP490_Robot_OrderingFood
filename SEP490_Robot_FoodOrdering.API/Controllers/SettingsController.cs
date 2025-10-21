using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("payment-policy")]
        [ProducesResponseType(typeof(BaseResponseModel<PaymentPolicy>), 200)]
        public async Task<ActionResult<BaseResponseModel>> GetPaymentPolicy()
        {
            var result = await _settingsService.GetPaymentPolicyAsync();
            return StatusCode(result.StatusCode, result);
        }

        public class UpdatePaymentPolicyRequest
        {
            public PaymentPolicy Policy { get; set; }
        }

        [HttpPatch("payment-policy")]
        [ProducesResponseType(typeof(BaseResponseModel<PaymentPolicy>), 200)]
        public async Task<ActionResult<BaseResponseModel>> UpdatePaymentPolicy([FromBody] UpdatePaymentPolicyRequest request)
        {
            var result = await _settingsService.UpdatePaymentPolicyAsync(request.Policy);
            return StatusCode(result.StatusCode, result);
        }
    }
}


