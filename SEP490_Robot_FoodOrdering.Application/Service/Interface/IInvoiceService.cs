using SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface;

public interface IInvoiceService
{
    Task<InvoiceResponse> createInvoice(InvoiceCreatRequest request);
}