using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Customer;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Customer;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/table")]
    [ApiController]
    public class TableCustomersController : ControllerBase
    {
        private readonly ITableCustomerService _tableCustomerService;

        public TableCustomersController(ITableCustomerService tableCustomerService)
        {
            _tableCustomerService = tableCustomerService;
        }

        /// <summary>
        /// Upsert customer theo SĐT và gắn vào TableSession Active (theo tableId + deviceId)
        /// </summary>
        /// <param name="tableId">Id của bàn</param>
        /// <param name="deviceId">Id thiết bị đang thao tác</param>
        /// <param name="req">Tên + SĐT</param>
        [HttpPost("{tableId:guid}/customer")]
        [ProducesResponseType(typeof(BaseResponseModel<BindCustomerToTableResult>), StatusCodes.Status200OK)]
        public async Task<BaseResponseModel<BindCustomerToTableResult>> BindCustomerToTable(
            [FromRoute] Guid tableId,
            [FromQuery] string deviceId,
            [FromBody] BindCustomerToTableRequest req)
        {
            var data = await _tableCustomerService.BindCustomerToActiveSessionAsync(tableId, deviceId, req);

            return new BaseResponseModel<BindCustomerToTableResult>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                data,
                "Cập nhật thông tin khách hàng vào bàn thành công"
            );
        }
        /// <summary>
        /// Check nhanh: bàn này đã đủ customer để checkout chưa (optional cho FE)
        /// </summary>
        [HttpGet("{tableId:guid}/customer/ready-for-checkout")]
        [ProducesResponseType(typeof(BaseResponseModel<object>), StatusCodes.Status200OK)]
        public async Task<BaseResponseModel<object>> IsReadyForCheckout([FromRoute] Guid tableId)
        {
            await _tableCustomerService.EnsureCustomerReadyForCheckoutAsync(tableId);

            return new BaseResponseModel<object>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                new { ready = true },
                "OK"
            );
        }

    }
}
