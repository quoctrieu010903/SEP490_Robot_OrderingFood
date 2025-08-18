using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.fedback;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;
using SEP490_Robot_FoodOrdering.Infrastructure.Repository;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation;

public class FeedbackService : IFeedbackService
{
    private readonly FeedbackMemoryStore _memoryStore;
    private bool _storeInitialized = false;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderService _orderService;


    public FeedbackService(IUnitOfWork unitOfWork, FeedbackMemoryStore memoryStore, IOrderService orderService)
    {
        _unitOfWork = unitOfWork;
        _memoryStore = memoryStore;
        _orderService = orderService;
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


    public async Task<BaseResponseModel<FeedbackCreate>> CreateFeedback(FeedbackRequest feedbackRequest)
    {
        List<object> feedbackList = await getStore(feedbackRequest.idTable);

        List<OrderItemDTO> dtos = new List<OrderItemDTO>();
        if (feedbackRequest.idOrderItem != null)
        {
            try
            {
                var items = await _items(feedbackRequest.idOrderItem);
                IDictionary<Guid, int> dictionary = new Dictionary<Guid, int>();
                items.ForEach(item =>
                    dtos.Add(new OrderItemDTO(item.Id, item.Product.Name, item.Product.ImageUrl, item.Status)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        var temp = new FeedbackModole(feedbackRequest.note, true, DateTime.Now, "", Guid.NewGuid(), dtos);

        feedbackList.Add(temp);

        _memoryStore.Store[feedbackRequest.idTable.ToString()] = feedbackList;

        return new BaseResponseModel<FeedbackCreate>
        (StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS,
            new FeedbackCreate(temp.CreatedTime, temp.IsPeeding, temp.Feedback));
    }


    protected async Task<List<OrderItem>> _items(List<Guid> ids)
    {
        var temp = await _unitOfWork.Repository<OrderItem, Guid>().GetAllWithIncludeAsync(true, item => item.Product);
        return temp.Where(item => ids.Contains(item.Id)).ToList();
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
                    feedback.CreatedTime, feedback.OrderItemDto));
            }
        }

        return new BaseResponseModel<List<FeedbackGet>>(
            StatusCodes.Status200OK,
            ResponseCodeConstants.SUCCESS,
            feedbackList
        );
    }


    public async Task<BaseResponseModel<List<FeedbackCreate>>> ConfirmFeedback(Guid idTable, List<Guid> IDFeedback,
        bool isPeeding, string content)
    {
        var feedbackList = await getStore(idTable) ?? new List<object>();

        var updatedFeedbacks = new List<FeedbackCreate>();
        bool found = false;

        foreach (var o in feedbackList)
        {
            if (o is FeedbackModole feedback && IDFeedback.Contains(feedback.IDFeedback))
            {
                // Cập nhật trạng thái
                feedback.IsPeeding = isPeeding;

                // Nếu muốn cập nhật nội dung thì gán luôn
                feedback.content = content;

                updatedFeedbacks.Add(
                    new FeedbackCreate(feedback.CreatedTime, feedback.IsPeeding, feedback.Feedback)
                );
                found = true;
            }
        }

        if (!found)
            throw new ErrorException(404, "No feedbacks found with given IDs");

        _memoryStore.Store[idTable.ToString()] = feedbackList;

        return new BaseResponseModel<List<FeedbackCreate>>(
            StatusCodes.Status200OK,
            ResponseCodeConstants.SUCCESS,
            updatedFeedbacks
        );
    }

    public async Task<BaseResponseModel<Dictionary<string, FeedbackPeedingInfo>>> GetAllFeedbackIsPeeding()
    {
        var tableCount = _memoryStore.Tables.Count;
        var feedbackList = new Dictionary<string, FeedbackPeedingInfo>(tableCount);

        var tableIds = _memoryStore.Tables.Keys.Select(Guid.Parse).ToArray();
        var orderStatsDict = await _orderService.GetOrderStatsByTableIds(tableIds);


        foreach (var table in _memoryStore.Tables)
        {
            var feedbacks = _memoryStore.Store.GetValueOrDefault(table.Key);

            int counter = 0;
            if (feedbacks != null)
            {
                foreach (var feedback in feedbacks)
                {
                    if (feedback is FeedbackModole { IsPeeding: true })
                        counter++;
                }
            }

            var tableGuid = Guid.Parse(table.Key);
            var orderStats = orderStatsDict[tableGuid];

            feedbackList[table.Key] = new FeedbackPeedingInfo(
                table.Value,
                counter,
                orderStats.DeliveredCount,
                orderStats.PaidCount,
                orderStats.TotalOrderItems
            );
        }


        var sorted = feedbackList
            .OrderBy(x => x.Value.TableName, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Value, feedbackList.Comparer);

        return new BaseResponseModel<Dictionary<string, FeedbackPeedingInfo>>(
            StatusCodes.Status200OK,
            ResponseCodeConstants.SUCCESS,
            sorted
        );
    }
}