using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.SystemSettings;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
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
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponseModel<IEnumerable<SystemSettingResponse>>), 200)]
        public async Task<ActionResult<BaseResponseModel>> GetAllSettings()
        {
            var result = await _settingsService.GetAllAsync();
            return StatusCode(result.StatusCode, result);

        }
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponseModel<SystemSettings>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponseModel<SystemSettings>>> GetSettingById(Guid id)
        {
            var result = await _settingsService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch]
        [ProducesResponseType(typeof(BaseResponseModel<bool>), 200)]
        public async Task<ActionResult<BaseResponseModel>> UpdateSettingByKey([FromQuery] string key, [FromQuery] string value)
        {
            var result = await _settingsService.UpdateValueAsync(key, value);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(BaseResponseModel<bool>), 200)]
        public async Task<ActionResult<BaseResponseModel>> UpdateSettingById([FromRoute] Guid id, [FromQuery] string value)
        {
            var result = await _settingsService.UpdateByIdAsync(id, value);
            return StatusCode(result.StatusCode, result);
        }
        
    }
}


