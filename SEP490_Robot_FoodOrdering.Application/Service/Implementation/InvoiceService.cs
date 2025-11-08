using System.Net.NetworkInformation;
using AutoMapper;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public Task<InvoiceResponse> createInvoice(InvoiceCreatRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<PaginatedList<InvoiceResponse>> getAllInvoice(PagingRequestModel pagingRequest )
    {
        throw new NotImplementedException();
    }

    public Task<InvoiceResponse> getInvoiceById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<PaginatedList<InvoiceResponse>> getInvoiceByTableId(Guid tableId, PagingRequestModel pagingRequest)
    {
        var specification = new InvoiceSpecification(tableId,pagingRequest.PageNumber,pagingRequest.PageSize);
        var invoices = await _unitOfWork.Repository<Invoice, Guid>().GetAllWithSpecAsync(specification);
        var response = _mapper.Map<List<InvoiceResponse>>(invoices);


        return PaginatedList<InvoiceResponse>.Create(response, pagingRequest.PageNumber, pagingRequest.PageSize);
    }

    private InvoiceDetailResponse CreateInvoiceDetailResponse(InvoiceDetail detail)
    {
        try
        {
            if (detail?.OrderItem == null)
                return null;

            var orderItem = detail.OrderItem;

            return new InvoiceDetailResponse()
            {
                OrderItemId = orderItem.Id,
                ProductName = orderItem.ProductSize?.Product?.Name ?? "Unknown Product",
                UnitPrice = orderItem.ProductSize?.Price ?? 0,
                Toppings = orderItem.OrderItemTopping?
                    .Where(t => t?.Topping != null)
                    .Select(t => t.Topping.Name ?? "Unknown Topping")
                    .ToList() ?? new List<string>(),
                TotalMoney = detail.TotalMoney
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating invoice detail response: {ex.Message}");
            return new InvoiceDetailResponse()
            {
                OrderItemId = detail?.OrderItem?.Id ?? Guid.Empty,
                ProductName = "Error Loading Product",
                UnitPrice = 0,
                Toppings = new List<string>(),
                TotalMoney = detail?.TotalMoney ?? 0
            };
        }
    }
}