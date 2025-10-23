using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Feedback;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponseModel> Create(CreateFeedbackRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_RATING_VALUE, "Rating must be 1-5");
            }

            var entity = _mapper.Map<Feedback>(request);
            entity.CreatedBy = null;
            entity.LastUpdatedBy = string.Empty;
            entity.CreatedTime = DateTime.UtcNow;
            entity.LastUpdatedTime = DateTime.UtcNow;

            await _unitOfWork.Repository<Feedback, Guid>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, entity.Id);
        }

        public async Task<BaseResponseModel> Delete(Guid id)
        {
            var existed = await _unitOfWork.Repository<Feedback, Guid>().GetByIdAsync(id);
            if (existed == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Feedback không tìm thấy");
            }
            existed.DeletedBy = string.Empty;
            existed.DeletedTime = DateTime.UtcNow;
            existed.LastUpdatedBy = string.Empty;
            existed.LastUpdatedTime = DateTime.UtcNow;
            _unitOfWork.Repository<Feedback, Guid>().Update(existed);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Xoá thành công");
        }

        public async Task<PaginatedList<FeedbackResponse>> GetAll(PagingRequestModel paging)
        {
            var list = await _unitOfWork.Repository<Feedback, Guid>().GetAllAsync();
            var responses = _mapper.Map<List<FeedbackResponse>>(list.OrderByDescending(x => x.CreatedTime).ToList());
            return PaginatedList<FeedbackResponse>.Create(responses, paging.PageNumber, paging.PageSize);
        }

        public async Task<PaginatedList<FeedbackResponse>> GetByTableId(Guid tableId, PagingRequestModel paging)
        {
            var list = await _unitOfWork.Repository<Feedback, Guid>().GetListAsync(x => x.TableId == tableId);
            var ordered = list.OrderByDescending(x => x.CreatedTime).ToList();
            var responses = _mapper.Map<List<FeedbackResponse>>(ordered);
            return PaginatedList<FeedbackResponse>.Create(responses, paging.PageNumber, paging.PageSize);
        }

        public async Task<PaginatedList<FeedbackResponse>> GetByOrderItemId(Guid orderItemId, PagingRequestModel paging)
        {
            var list = await _unitOfWork.Repository<Feedback, Guid>().GetListAsync(x => x.OrderItemId == orderItemId);
            var ordered = list.OrderByDescending(x => x.CreatedTime).ToList();
            var responses = _mapper.Map<List<FeedbackResponse>>(ordered);
            return PaginatedList<FeedbackResponse>.Create(responses, paging.PageNumber, paging.PageSize);
        }

        public async Task<BaseResponseModel<FeedbackResponse>> GetById(Guid id)
        {
            var existed = await _unitOfWork.Repository<Feedback, Guid>().GetByIdAsync(id);
            if (existed == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Feedback không tìm thấy");
            }
            var resp = _mapper.Map<FeedbackResponse>(existed);
            return new BaseResponseModel<FeedbackResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, resp);
        }

        public async Task<BaseResponseModel<FeedbackResponse>> Update(Guid id, UpdateFeedbackRequest request)
        {
            var existed = await _unitOfWork.Repository<Feedback, Guid>().GetByIdAsync(id);
            if (existed == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Feedback không tìm thấy");
            }
            if (request.Rating.HasValue)
            {
                if (request.Rating.Value < 1 || request.Rating.Value > 5)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_RATING_VALUE, "Rating must be 1-5");
                }
                existed.Rating = request.Rating.Value;
            }

            if (request.Comment != null)
            {
                existed.Comment = request.Comment;
            }

            if (request.Type.HasValue)
            {
                existed.Type = (Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities.FeedbackTypeEnum)request.Type.Value;
            }
            existed.LastUpdatedBy = string.Empty;
            existed.LastUpdatedTime = DateTime.UtcNow;

            await _unitOfWork.Repository<Feedback, Guid>().UpdateAsync(existed);
            await _unitOfWork.SaveChangesAsync();
            var resp = _mapper.Map<FeedbackResponse>(existed);
            return new BaseResponseModel<FeedbackResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, resp);
        }
    }
}


