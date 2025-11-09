using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
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
    [HttpGet("table/{tableId}")]
    public async Task<ActionResult<PaginatedList<InvoiceResponse>>> GetInvoicesByTableId(Guid tableId, [FromQuery] PagingRequestModel pagingRequest)
    {
        var result = await _invoiceService.getInvoiceByTableId(tableId, pagingRequest);
        return result;
    }
}