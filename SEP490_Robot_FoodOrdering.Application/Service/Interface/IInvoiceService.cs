using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface;

public interface IInvoiceService
{
    Task<InvoiceResponse> CreateInvoice(InvoiceCreatRequest request);
    Task<PaginatedList<InvoiceResponse>> getAllInvoice(PagingRequestModel pagingRequest);
    Task<BaseResponseModel<InvoiceResponse>> getInvoiceById(Guid id);
    Task<BaseResponseModel<InvoiceResponse>> getInvoiceByTableId(Guid OrderId);
    Task<BaseResponseModel<LatestInvoiceByPhoneResponse>> GetLatestInvoiceByPhoneAsync(string phone);


}