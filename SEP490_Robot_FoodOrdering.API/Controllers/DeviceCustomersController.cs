using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Customer;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/devices")]
    [ApiController]
    public class DeviceCustomersController : ControllerBase
    {
        private readonly ITableCustomerService _tableCustomerService;

        public DeviceCustomersController(ITableCustomerService tableCustomerService)
        {
            _tableCustomerService = tableCustomerService;
        }

        /// <summary>
        /// Lấy customer của session active mới nhất theo deviceId (có thể null)
        /// </summary>
        [HttpGet("{deviceId}/active-customer")]
        [ProducesResponseType(typeof(BaseResponseModel<CustomerResponse?>), StatusCodes.Status200OK)]
        public async Task<BaseResponseModel<CustomerResponse?>> GetActiveCustomerByDevice(
            [FromRoute] string deviceId)
        {
            var data = await _tableCustomerService.GetActiveCustomerByDeviceIdAsync(deviceId);

            return new BaseResponseModel<CustomerResponse?>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                data,
                data == null ? "Thiết bị chưa có khách trong session active" : "Lấy khách theo thiết bị thành công"
            );
        }
    }
}
