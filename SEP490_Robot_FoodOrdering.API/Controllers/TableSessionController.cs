using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.TableActivities;
using SEP490_Robot_FoodOrdering.Core.Response;
using Microsoft.AspNetCore.Authorization;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableSessionController : ControllerBase
    {
        private readonly ILogger<TableSessionController> _logger;

        private readonly ITableSessionService _service;
        private readonly ITableShare _tableShare;

        public TableSessionController(ILogger<TableSessionController> logger, ITableSessionService service, ITableShare tableShare)
        {
            _logger = logger;
            _service = service;
            _tableShare = tableShare;
        }

        [HttpGet("TableId/{TableId}")]
        public async Task<ActionResult<PaginatedList<TableActivityLogResponse>>> GetAll( Guid TableId,[FromQuery] PagingRequestModel paging)
        {
            var result = await _service.GetSessionByTableId(TableId, paging);
            return Ok(result);
        }
        [HttpPatch("TableId/{tableId}/attach-device")]
        [Authorize]
        public async Task<IActionResult> AttachDevice(Guid tableId, [FromBody] AttachDeviceRequest request)
        {
            var result = await _tableShare.AttachDeviceAsync(tableId, request);
            return Ok(result);
        }
    }
}
