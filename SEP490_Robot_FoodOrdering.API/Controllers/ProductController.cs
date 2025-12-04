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
    /// <summary>
    /// Product management controller for handling food and beverage items
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly IProductToppingService _toppingservice;

        /// <summary>
        /// Initialize Product Controller with required services
        /// </summary>
        /// <param name="service">Product service for managing products</param>
        /// <param name="toppingService">Topping service for managing product toppings</param>
        public ProductController(IProductService service, IProductToppingService toppingService)
        {
            _service = service;
            _toppingservice = toppingService;
        }

        /// <summary>
        /// Get all products with pagination and filtering options
        /// </summary>
        /// <param name="paging">Pagination parameters including page number and page size</param>
        /// <param name="fillter">Product filtering parameters such as categoryName,Search : can Search flowing by ProductName or description of Product  </param>
        /// <returns>Paginated list of products with total count and page information</returns>
        /// <response code="200">Successfully retrieved products list</response>
        /// <response code="400">Invalid pagination or filter parameters</response>
        /// <response code="500">Internal server error occurred</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/Product?pageNumber=1&amp;pageSize=10&amp;categoryName="Đồ Uống" &amp
        /// 
        /// This endpoint supports:
        /// - Pagination with configurable page size
        /// - Filtering by category, duration range, "Search" flowing by ProductName or description of Product
        /// - Sorting by name, price, creation date
        /// </remarks>
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ProductResponse>>> GetAll([FromQuery] PagingRequestModel paging, [FromQuery] ProductSpecParams fillter)
        {
            var result = await _service.GetAll(paging, fillter);
            return Ok(result);
        }

        /// <summary>
        /// Get detailed information of a specific product by its ID
        /// </summary>
        /// <param name="id">Unique identifier of the product</param>
        /// <returns>Detailed product information including category, pricing, and availability</returns>
        /// <response code="200">Successfully retrieved product details</response>
        /// <response code="404">Product with specified ID not found</response>
        /// <response code="400">Invalid product ID format</response>
        /// <response code="500">Internal server error occurred</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/Product/4ae13a6b-eeb1-4089-ba41-cc661da91d4a
        /// 
        /// Returns comprehensive product information including:
        /// - Basic product details (name, description, price, image , list sizes)
        /// - Category information
        
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel<ProductDetailResponse>>> GetById(Guid id)
        {
            var result = await _service.GetById(id);
            return Ok(result);
        }

        /// <summary>
        /// Create a new product in the system
        /// </summary>
        /// <param name="request">Product creation request containing name, price, category, and other details</param>
        /// <returns>Result of product creation operation</returns>
        /// <response code="201">Product created successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="404">Referenced category not found</response>
        /// <response code="409">Product with same name already exists</response>
        /// <response code="500">Internal server error occurred</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Product
        ///     {
        ///         "productName": "Cà Phê Sữa Đá",
        ///         "productName": "Traditional Vietnamese iced coffee with condensed milk",
        ///         "durationTime": 15,time to cooking        
        ///         "imageUrl": "https://example.com/coffee.jpg",
        ///     }
        /// 
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> Create([FromForm] CreateProductRequest request)
        {
            var result = await _service.Create(request);
            return Ok(result);
        }

        /// <summary>
        /// Get all available toppings for a specific product
        /// </summary>
        /// <param name="id">Product ID to retrieve toppings for</param>
        /// <returns>Product information with list of available toppings including prices</returns>
        /// <response code="200">Successfully retrieved product with toppings</response>
        /// <response code="404">Product with specified ID not found</response>
        /// <response code="400">Invalid product ID format</response>
        /// <response code="500">Internal server error occurred</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/Product/4ae13a6b-eeb1-4089-ba41-cc661da91d4a/toppings
        /// 
        /// Returns:
        /// - Product basic information (ID, name, price)
        /// - List of available toppings with individual prices
        /// - Topping details (name, price, image, availability)
        /// 
        /// This endpoint is commonly used for:
        /// - Displaying customization options to customers
        /// - Building order forms with topping selections
        /// - Calculating total prices including toppings
        /// </remarks>
        [HttpGet("{id}/toppings")]
        public async Task<IActionResult> GetProductToppings(Guid id)
        {
            var result = await _toppingservice.GetProductWithToppingsAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Update an existing product with new information
        /// </summary>
        /// <param name="request">Product update request with modified field values</param>
        /// <param name="id">ID of the product to update</param>
        /// <returns>Updated product information</returns>
        /// <response code="200">Product updated successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="404">Product with specified ID not found</response>
        /// <response code="409">Product name conflicts with existing product</response>
        /// <response code="500">Internal server error occurred</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/Product/4ae13a6b-eeb1-4089-ba41-cc661da91d4a
        ///     {
        ///         "name": "Cà Phê Sữa Đá (Updated)",
        ///         "description": "Premium Vietnamese iced coffee with condensed milk",
        ///         "price": 30000,
        ///         "categoryId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "imageUrl": "https://example.com/premium-coffee.jpg",
        ///         "isAvailable": true
        ///     }
        /// 
        /// Update rules:
        /// - All fields in the request will overwrite existing values
        /// - Name uniqueness is validated (excluding current product)
        /// - Category must exist if changed
        /// - Price validation applies same as creation
        /// - Last modified timestamp is automatically updated
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel<ProductResponse>>> Update([FromForm] CreateProductRequest request, Guid id)
        {
            var result = await _service.Update(request, id);
            return Ok(result);
        }

        /// <summary>
        /// Soft delete a product by updating its LastUpdatedTime and DeletedTime
        /// </summary>
        /// <param name="id">ID of the product to delete</param>
        /// <returns>Result of delete operation</returns>
        /// <response code="200">Product deleted (soft) successfully</response>
        /// <response code="404">Product with specified ID not found</response>
        /// <response code="400">Invalid product ID format</response>
        /// <response code="500">Internal server error occurred</response>
        /// <remarks>
        /// This operation does NOT remove the record from the database.
        /// It only updates LastUpdatedTime and DeletedTime fields for soft delete.
        /// </remarks>
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseModel>> Delete(Guid id)
        {
            var result = await _service.Delete(id);
            return Ok(result);
        }
    }
}