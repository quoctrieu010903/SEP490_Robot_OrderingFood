using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly ITableService _service;
        public TableController(ITableService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTableRequest request)
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
        /// <summary>
        /// Retrieve a paginated list of tables with optional status filtering.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of tables with support for:
        /// 
        /// * Pagination (page index and page size)
        /// * Filtering by table status
        /// 
        /// Sample request:
        /// GET /api/Table?PageIndex=1&PageSize=10&status=Available
        /// 
        /// Available table statuses:
        /// * Available — The table is free and ready to use
        /// * Occupied — The table is currently in use
        /// * Reserved — The table has been reserved
        /// 
        /// Use this endpoint to:
        /// * Display tables to staff or customers
        /// * Monitor real-time table status
        /// * Build dashboard or booking views
        /// </remarks>
        /// <param name="paging">Paging parameters (PageIndex, PageSize)</param>
        /// <param name="status">Optional status filter (Available, Occupied, Reserved)</param>
        /// <returns>A paginated list of tables matching the filter</returns>
        /// <response code="200">List of tables retrieved successfully</response>
        /// <response code="400">Invalid query parameters</response>
        /// <response code="500">Internal server error</response>
        public async Task<IActionResult> GetAll([FromQuery] PagingRequestModel paging, [FromQuery] TableEnums? status)
        {
            var result = await _service.GetAll(paging, status);
            return Ok(result);
        }


        [HttpGet("{id}")]
    
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetById(id);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateTableRequest request)
        {
            var result = await _service.Update(request, id);
            return Ok(result);
        }
    }
} 