using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using System;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductToppingController : ControllerBase
    {
        private readonly IProductToppingService _service;
        public ProductToppingController(IProductToppingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductToppingRequest request)
        {
            var result = await _service.Create(request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.Delete(id);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagingRequestModel paging)
        {
            var result = await _service.GetAll(paging);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetById(id);
            return Ok(result);
        }
       

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateProductToppingRequest request)
        {
            var result = await _service.Update(request, id);
            return Ok(result);
        }
    }
} 