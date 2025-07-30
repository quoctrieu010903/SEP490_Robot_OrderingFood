using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.Repository;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation;

public class FeedbackService : IFeedbackService
{
    private readonly FeedbackMemoryStore _memoryStore;
    private bool _storeInitialized = false;
    private readonly IUnitOfWork _unitOfWork;

    public FeedbackService(IUnitOfWork unitOfWork, FeedbackMemoryStore memoryStore)
    {
        _unitOfWork = unitOfWork;
        _memoryStore = memoryStore;
    }


    private async Task EnsureStoreInitialized()
    {
        if (_storeInitialized) return;

        var allTables = await _unitOfWork.Repository<Table, Guid>().GetAllAsync();
        foreach (var table in allTables)
        {
            if (!_memoryStore.Store.ContainsKey(table.Id.ToString()))
            {
                _memoryStore.Store[table.Id.ToString()] = new List<object>();
            }
        }

        _storeInitialized = true;
    }

    protected async Task<List<object>> getStore(Guid idTable)
    {
        await EnsureStoreInitialized();

        if (!_memoryStore.Store.TryGetValue(idTable.ToString(), out var feedbackList))
        {
            feedbackList = new List<object>();
            _memoryStore.Store[idTable.ToString()] = feedbackList;
        }

        return feedbackList;
    }


    public async Task<BaseResponseModel<FeedbackCreate>> CreateFeedback(Guid idTable, string feedback)
    {
        List<object> feedbackList = await getStore(idTable);

        var temp = new FeedbackModole(feedback, true, DateTime.Now, Guid.NewGuid());

        feedbackList.Add(temp);

        _memoryStore.Store[idTable.ToString()] = feedbackList;

        return new BaseResponseModel<FeedbackCreate>
        (StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS,
            new FeedbackCreate(temp.CreatedTime, temp.IsPeeding, temp.Feedback));
    }


    public async Task<BaseResponseModel<List<FeedbackGet>>> GetFeedbackTable(Guid idTable)
    {
        List<object> temp = await getStore(idTable);


        List<FeedbackGet> feedbackList = new List<FeedbackGet>();
        foreach (var o in temp)
        {
            Console.Write(o.ToString());
            if (o is FeedbackModole feedback)
            {
                feedbackList.Add(new FeedbackGet(feedback.IDFeedback, idTable, feedback.Feedback, feedback.IsPeeding,
                    feedback.CreatedTime));
            }
        }

        return new BaseResponseModel<List<FeedbackGet>>(
            StatusCodes.Status200OK,
            ResponseCodeConstants.SUCCESS,
            feedbackList
        );
    }


    public async Task<BaseResponseModel<Dictionary<string, FeedbackPeedingInfo>>> GetAllFeedbackIsPeeding()
    {
        Dictionary<string, FeedbackPeedingInfo> feedbackList = new Dictionary<string, FeedbackPeedingInfo>();
        var tableList = await _unitOfWork.Repository<Table, Guid>().GetAllAsync();

        foreach (var table in tableList)
        {
            var key = table.Id.ToString();

            _memoryStore.Store.TryGetValue(key, out var feedback);
            int counter = feedback?.Count ?? 0;

            feedbackList[key] = new FeedbackPeedingInfo(Counter: counter, TableName: table.Name);
        }

        return new BaseResponseModel<Dictionary<string, FeedbackPeedingInfo>>(StatusCodes.Status200OK,
            ResponseCodeConstants.SUCCESS, feedbackList);
    }

    public async Task<BaseResponseModel<FeedbackCreate>> ConfirmFeedback(Guid idTable, Guid IDFeedback, bool isPeeding)
    {
        List<object> feedbackList = await getStore(idTable);

        foreach (var o in feedbackList)
        {
            if (o is FeedbackModole feedback && feedback.IDFeedback == IDFeedback)
            {
                feedback.IsPeeding = isPeeding;
                return new BaseResponseModel<FeedbackCreate>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    new FeedbackCreate(feedback.CreatedTime, feedback.IsPeeding, feedback.Feedback)
                );
            }
        }

        throw new ErrorException(404, "Feedback not found");
    }
}