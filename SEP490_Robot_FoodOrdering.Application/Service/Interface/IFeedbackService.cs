using SEP490_Robot_FoodOrdering.Application.DTO.Request.fedback;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface;

public interface IFeedbackService
{
    Task<BaseResponseModel<FeedbackCreate>> CreateFeedback(FeedbackRequest feedbackRequest);
    Task<BaseResponseModel<List<FeedbackGet>>> GetFeedbackTable(Guid idTable);
    Task<BaseResponseModel<Dictionary<string, FeedbackPeedingInfo>>> GetAllFeedbackIsPeeding();
    Task<BaseResponseModel<List<FeedbackCreate>>> ConfirmFeedback(Guid idTable, List<Guid> IDFeedback, bool isPeeding);
}