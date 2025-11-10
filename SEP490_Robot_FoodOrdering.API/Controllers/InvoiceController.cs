using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoiceController
{
    private readonly IInvoiceService _invoiceService;


    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<InvoiceResponse> createInvoice([FromBody] InvoiceCreatRequest request)
    {
        return await _invoiceService.CreateInvoice(request);
    }
    [HttpGet("Order/{OrderId}")]
    public async Task<ActionResult<BaseResponseModel<InvoiceResponse>>> GetInvoicesByTableId(Guid OrderId, [FromQuery] PagingRequestModel pagingRequest)
    {
        var result = await _invoiceService.getInvoiceByTableId(OrderId, pagingRequest);
        return result;
    }
}