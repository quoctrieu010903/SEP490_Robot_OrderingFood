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
        if (_memoryStore.Tables == null || !_memoryStore.Tables.Any())
        {
            _memoryStore.Tables = new Dictionary<string, string>();

            var allTables = await _unitOfWork.Repository<Table, Guid>().GetAllAsync();
            foreach (var allTable in allTables)
            {
                _memoryStore.Tables[allTable.Id.ToString()] = allTable.Name;
            }
        }

        foreach (var table in _memoryStore.Tables)
        {
            if (!_memoryStore.Store.ContainsKey(table.Key))
            {
                _memoryStore.Store[table.Key] = new List<object>();
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
        Dictionary<string, FeedbackPeedingInfo> feedbackList = new();

        foreach (var table in _memoryStore.Tables)
        {
            _memoryStore.Store.TryGetValue(table.Key, out var feedbacks);

            int counter = feedbacks?.Count(f =>
                f is FeedbackModole fb && fb.IsPeeding
            ) ?? 0;

            feedbackList[table.Key] = new FeedbackPeedingInfo(table.Value, counter);
        }

        var sorted = feedbackList
            .OrderBy(x => x.Value.TableName)
            .ToDictionary(x => x.Key, x => x.Value);

        return new BaseResponseModel<Dictionary<string, FeedbackPeedingInfo>>(StatusCodes.Status200OK,
            ResponseCodeConstants.SUCCESS, sorted);
    }


    public async Task<BaseResponseModel<List<FeedbackCreate>>> ConfirmFeedback(Guid idTable, List<Guid> IDFeedback,
        bool isPeeding)
    {
        var feedbackList = await getStore(idTable) ?? new List<object>();

        var updatedFeedbacks = new List<FeedbackCreate>();
        bool found = false;

        foreach (var o in feedbackList)
        {
            if (o is FeedbackModole feedback && IDFeedback.Contains(feedback.IDFeedback))
            {
                feedback.IsPeeding = isPeeding;
                updatedFeedbacks.Add(new FeedbackCreate(feedback.CreatedTime, feedback.IsPeeding, feedback.Feedback));
                found = true;
            }
        }

        if (!found)
            throw new ErrorException(404, "No feedbacks found with given IDs");

        return new BaseResponseModel<List<FeedbackCreate>>(
            StatusCodes.Status200OK,
            ResponseCodeConstants.SUCCESS,
            updatedFeedbacks
        );
    }
}