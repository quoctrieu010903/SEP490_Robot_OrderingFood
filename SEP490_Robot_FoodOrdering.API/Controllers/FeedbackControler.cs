using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeedbackControler
{
    public readonly IFeedbackService FeedbackService;

    public FeedbackControler(IFeedbackService feedbackService = null)
    {
        FeedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
    }


    [HttpPost("{idTable}")]
    public Task<BaseResponseModel<FeedbackCreate>> CreateFeedback(Guid idTable, [FromQuery] string feedback)
        => FeedbackService.CreateFeedback(idTable, feedback);


    [HttpGet("{idTable}")]
    public Task<BaseResponseModel<List<FeedbackGet>>> GetFeedbackTable(Guid idTable)
        => FeedbackService.GetFeedbackTable(idTable);

    [HttpPut("{idTable}")]
    public Task<BaseResponseModel<List<FeedbackCreate>>> ConfirmFeedback(
        Guid idTable,
        [FromQuery] List<Guid> idFeedback,
        [FromQuery] bool isPeeding)
        => FeedbackService.ConfirmFeedback(idTable, idFeedback, isPeeding);


    [HttpGet]
    public Task<BaseResponseModel<Dictionary<string, FeedbackPeedingInfo>>> GetAllFeedbackIsPeeding()
        => FeedbackService.GetAllFeedbackIsPeeding();
}