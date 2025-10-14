using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class CancelleditemsController : ControllerBase
    {
        private readonly ICancelledItemService _cancelledItemService;

        public CancelleditemsController(ICancelledItemService cancelledItemService)
        {
            _cancelledItemService = cancelledItemService;
        }
        /// <summary>
        /// Lấy tất cả món bị hủy có thể lọc theo ngày, người hủy, order
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CancelledItemFilterRequestParam filter)
        {
            var result = await _cancelledItemService.getAllCancelledItems(filter);
            return Ok(result); // Wrap the result in an OkObjectResult to return it as IActionResult
        }
        /// <summary>
        /// Create order item cancel
        ///</summary>
        [HttpPost("{orderItemId:guid}")]
        public async Task<IActionResult> CreateCancelledItem([FromRoute] Guid orderItemId, [FromQuery] string? cancelNote, [FromQuery] Guid cancelledByUserId)
        {
            var result = await _cancelledItemService.CreateCancelledItemAsync(orderItemId, cancelNote, cancelledByUserId);
            return Ok(result); 
        }
    }
}
