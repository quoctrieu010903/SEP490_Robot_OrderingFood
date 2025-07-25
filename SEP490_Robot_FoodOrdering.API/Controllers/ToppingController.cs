using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToppingController : ControllerBase
    {
        private readonly IToppingService _service;

        public ToppingController(IToppingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedList<ToppingResponse>>> GetAll([FromQuery] PagingRequestModel paging)
        {
            var result = await _service.GetAllToppingsAsync(paging);
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<BaseResponseModel<ToppingResponse>>> GetByIdAsync(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> CreateToppingAsync([FromBody] CreateToppingRequest request)
        {
            var result = await _service.Create(request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseModel>> DeleteToppingAsync(Guid id)
        {
            var result = await _service.Delete(id);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel>> UpdateToppingAsync([FromBody] CreateToppingRequest request,
            Guid id)
        {
            var result = await _service.Update(request, id);
            return Ok(result);
        }

        [HttpGet("production/{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetProductionAsync(Guid id)
        {
            var temp = await _service.GetByIdProduction(id);
            return Ok(temp);
        }
    }
}