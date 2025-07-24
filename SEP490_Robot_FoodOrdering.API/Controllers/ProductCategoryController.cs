
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryService _service;
        public ProductCategoryController(IProductCategoryService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ProductCategoryResponse>>> GetAll([FromQuery] PagingRequestModel paging)
        {
            var result = await _service.GetAllProductCategoriesAsync(paging);
            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> CreateProductCategoryAsync([FromBody] CreateProductCategoryRequest request)
        {
            var result = await _service.CreateProductCategoryAsync(request.ProductId, request.CategoryId);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseModel>> DeleteProductCategoryAsync(Guid id)
        {
            var result = await _service.DeleteProductCategoryAsync(id);
            return Ok(result);
        }
    }
}