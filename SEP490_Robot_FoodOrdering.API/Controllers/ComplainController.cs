using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComplainController
{
    public readonly IComplainService FeedbackService;

    public ComplainController(IComplainService feedbackService = null)
    {
        FeedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
    }


    [HttpPost]
    public Task<BaseResponseModel<ComplainCreate>> CreateFeedback([FromBody] ComplainRequests request)
    {
       var response =  FeedbackService.CreateComplainAsyns(request);
        return response;
    }


    [HttpGet("{idTable}")]
    public Task<BaseResponseModel<List<ComplainResponse>>> GetFeedbackTable(Guid idTable , [FromQuery] bool isCustomer)
        => FeedbackService.GetComplainByTable(idTable,isCustomer);

    [HttpPut("{idTable}")]
    [Authorize()]
    public Task<BaseResponseModel<List<ComplainCreate>>> ConfirmFeedback(
        Guid idTable,
        [FromQuery] List<Guid>? idFeedback,
        [FromQuery] string content,
        [FromQuery] bool isPeeding)
        => FeedbackService.ComfirmComplain(idTable, idFeedback, isPeeding, content);


    [HttpGet]
    public Task<BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>> GetAllFeedbackIsPeeding()
        => FeedbackService.GetAllComplainIsPending();

    // ================== QUICK SERVE ENDPOINTS (cho waiter) ==================

    /// <summary>
    /// Lấy tất cả QuickServeItems pending cho waiter (gồm Id, TableId, TableName, ItemName, IsServed...)
    /// </summary>
    [HttpGet("QuickServe/pending")]
    //[Authorize(Roles = "Waiter,Moderator,Admin")]
    public Task<BaseResponseModel<List<QuickServeItemResponse>>> GetPendingQuickServeItems()
        => FeedbackService.GetPendingQuickServeItemsAsync();

    /// <summary>
    /// Lấy tất cả QuickServeItems đã phục vụ để hiển thị ở tab "Đã phục vụ".
    /// </summary>
    [HttpGet("QuickServe/served")]
    //[Authorize(Roles = "Waiter,Moderator,Admin")]
    public Task<BaseResponseModel<List<QuickServeItemResponse>>> GetServedQuickServeItems()
        => FeedbackService.GetServedQuickServeItemsAsync();

    /// <summary>
    /// Đánh dấu một QuickServeItem đã được phục vụ.
    /// </summary>
    [HttpPost("QuickServe/serve/{quickServeItemId:guid}")]
    //[Authorize(Roles = "Waiter,Moderator,Admin")]
    public Task<BaseResponseModel<bool>> ServeQuickServeItem(Guid quickServeItemId)
        => FeedbackService.ServeQuickServeItemAsync(quickServeItemId);
}