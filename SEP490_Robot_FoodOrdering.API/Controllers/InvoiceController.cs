using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;

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
    public async Task<InvoiceResponse> createInvoice([FromForm] InvoiceCreatRequest request)
    {
        return await _invoiceService.createInvoice(request);
    }
}