using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Fillter;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private readonly IProductService _service;
        public ProductController(IProductService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ProductResponse>>> GetAll([FromQuery] PagingRequestModel paging, [FromQuery] ProductSpecParams fillter)
        {
            var result = await _service.GetAll(paging, fillter);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel<ProductDetailResponse>>> GetById(Guid id)
        {
            var result = await _service.GetById(id);
            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> Create([FromBody] CreateProductRequest request)
        {

            var result = await _service.Create(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel<ProductResponse>>> Update([FromBody] CreateProductRequest request, Guid id)
        {
            var result = await _service.Update(request, id);
            return Ok(result);
        }
    }
}
