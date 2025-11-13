using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableActivityController : ControllerBase
    {
        private readonly ITableActivityService _tableActivities;
        public TableActivityController(ITableActivityService tableActivities)
        {
            _tableActivities = tableActivities;
        }
        [HttpGet("{sessionId:guid}")]
        public async Task<IActionResult> GetAction(Guid sessionId, [FromQuery] PagingRequestModel paging)
        {
        
            var logs = await _tableActivities.GetLogAsync(sessionId , paging);

           return Ok(logs);
        }

    }
}
