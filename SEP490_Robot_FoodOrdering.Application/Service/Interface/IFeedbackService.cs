using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Feedback;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IFeedbackService
    {
        Task<BaseResponseModel> Create(CreateFeedbackRequest request);
        Task<BaseResponseModel<FeedbackResponse>> Update(Guid id, UpdateFeedbackRequest request);
        Task<BaseResponseModel<FeedbackResponse>> GetById(Guid id);
        Task<PaginatedList<FeedbackResponse>> GetAll(PagingRequestModel paging);
        Task<PaginatedList<FeedbackResponse>> GetByTableId(Guid tableId, PagingRequestModel paging);
        //Task<PaginatedList<FeedbackResponse>> GetByOrderItemId(Guid tableid, PagingRequestModel paging);
        Task<BaseResponseModel> Delete(Guid id);
    }
}
