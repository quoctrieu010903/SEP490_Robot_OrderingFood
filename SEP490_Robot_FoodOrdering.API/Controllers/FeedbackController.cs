﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Feedback;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedList<FeedbackResponse>>> GetAll([FromQuery] PagingRequestModel paging)
        {
            var result = await _feedbackService.GetAll(paging);
            return Ok(result);
        }

        [HttpGet("table/{tableId}")]
        public async Task<ActionResult<PaginatedList<FeedbackResponse>>> GetByTableId(Guid tableId, [FromQuery] PagingRequestModel paging)
        {
            var result = await _feedbackService.GetByTableId(tableId, paging);
            return Ok(result);
        }

        [HttpGet("order-item/{orderItemId}")]
        public async Task<ActionResult<PaginatedList<FeedbackResponse>>> GetByOrderItemId(Guid orderItemId, [FromQuery] PagingRequestModel paging)
        {
            var result = await _feedbackService.GetByOrderItemId(orderItemId, paging);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel<FeedbackResponse>>> GetById(Guid id)
        {
            var result = await _feedbackService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> Create([FromBody] CreateFeedbackRequest request)
        {
            var result = await _feedbackService.Create(request);
            return Ok(result);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<BaseResponseModel<FeedbackResponse>>> Update(Guid id, [FromBody] UpdateFeedbackRequest request)
        {
            var result = await _feedbackService.Update(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseModel>> Delete(Guid id)
        {
            var result = await _feedbackService.Delete(id);
            return Ok(result);
        }
        
    }
}
