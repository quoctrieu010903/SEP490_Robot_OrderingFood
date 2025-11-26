using SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface;

public interface IComplainService
{
    Task<BaseResponseModel<ComplainCreate>> CreateComplainAsyns(ComplainRequests request);
    Task<BaseResponseModel<List<ComplainResponse>>> GetComplainByTable(Guid idTable, bool forCustomer = false);
    Task<BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>> GetAllComplainIsPending();
    Task<BaseResponseModel<List<ComplainCreate>>> ComfirmComplain(Guid idTable, List<Guid>? IDFeedback, bool isPeeding,string content);
}