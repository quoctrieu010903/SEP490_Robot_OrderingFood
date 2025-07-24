using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Security;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedList<ProductCategoryResponse>>> GetAll([FromQuery] PagingRequestModel paging)
        {
            var result = await _service.GetAllCategory(paging);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel<CategoryResponse>>> GetById(Guid id)
        {
            var result = await _service.GetCategoryById(id);
          
            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> Create([FromBody] CreateCategoryRequest request)
        {
            var result = await _service.Create(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel>> Update([FromBody] CreateCategoryRequest request, Guid id)
        {
            var result = await _service.Update(request, id);
            return Ok(result);
        }

    }
}
