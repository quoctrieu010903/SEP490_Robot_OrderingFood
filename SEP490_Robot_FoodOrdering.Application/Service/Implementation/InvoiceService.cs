using System.Net.NetworkInformation;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
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
    private readonly IUtilsService _utilsService;

    public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper, IUtilsService utilsService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _utilsService = utilsService;
    }

    public async Task<InvoiceResponse> CreateInvoice(InvoiceCreatRequest request)
    {
        var existedOrder = await _unitOfWork.Repository<Order, Guid>()
            .GetWithSpecAsync(new OrderSpecification(request.OrderId, true));

        if (existedOrder == null)
            throw new ErrorException(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found");

        if (existedOrder.PaymentStatus != PaymentStatusEnums.Paid)
            throw new ErrorException(StatusCodes.Status400BadRequest, "ORDER_NOT_PAID", "Order is not paid yet");

        // (Khuyến nghị) chống tạo trùng theo OrderId
        var existedInvoice = await _unitOfWork.Repository<Invoice, Guid>()
            .GetWithSpecAsync(new BaseSpecification<Invoice>(x => x.OrderId == existedOrder.Id));

        if (existedInvoice != null)
            return BuildInvoiceResponse(existedInvoice, existedOrder);

        // ✅ Tạo ID trước để detail dùng FK chuẩn
        //var invoiceId = Guid.NewGuid();

        var invoice = new Invoice
        {
            //Id = invoiceId, // ✅ QUAN TRỌNG
            OrderId = existedOrder.Id,
            TableId = existedOrder.TableId ?? request.TableId,
            InvoiceCode = _utilsService.GenerateCode("HD", 6),
            CreatedTime = DateTime.UtcNow,
            TotalMoney = existedOrder.TotalPrice,
            PaymentMethod = existedOrder.paymentMethod,
            Status = existedOrder.PaymentStatus,
            Details = new List<InvoiceDetail>() // ✅ tránh null
        };

        var orderStatus = existedOrder.Status;
        if (existedOrder.OrderItems != null)
        {
            foreach (var oi in existedOrder.OrderItems)
            {
                var detail = new InvoiceDetail
                {
                    //Id = Guid.NewGuid(),
                    //InvoiceId = invoiceId, // ✅ FK đúng
                    Invoices = invoice,    // ✅ navigation đúng (theo entity của bạn)
                    OrderItemId = oi.Id,
                    TotalMoney = oi.TotalPrice ?? 0,
                    Status = orderStatus  // ✅
                };

                invoice.Details.Add(detail);
            }
        }

        await _unitOfWork.Repository<Invoice, Guid>().AddAsync(invoice);
        // ❌ KHÔNG SaveChanges ở đây (CheckoutTable sẽ SaveChanges ở cuối)

        return BuildInvoiceResponse(invoice, existedOrder);
    }

    private InvoiceResponse BuildInvoiceResponse(Invoice invoice, Order existedOrder)
    {
        return new InvoiceResponse
        {
            Id = invoice.Id,
            OrderId = existedOrder.Id,
            TableId = existedOrder.TableId ?? Guid.Empty,
            TableName = existedOrder.Table?.Name,
            InvoiceCode = invoice.InvoiceCode,
            CreatedTime = DateTime.UtcNow, // hoặc truyền createdTime vào nếu muốn đúng tuyệt đối
            PaymentMethod = existedOrder.paymentMethod.ToString(),
            TotalAmount = existedOrder.TotalPrice,
            Discount = 0,
            FinalAmount = existedOrder.TotalPrice,
            CashierName = "Moderator Manager",
            Details = existedOrder.OrderItems?.Select(oi => new InvoiceDetailResponse
            {
                OrderItemId = oi.Id,
                ProductName = oi.Product?.Name,
                UnitPrice = oi.Price ?? 0,
                toppings = oi.OrderItemTopping?.Select(oit => new ToppingResponse
                {
                    Id = oit.Topping.Id,
                    Name = oit.Topping.Name,
                    Price = oit.Topping.Price
                }).ToList() ?? new List<ToppingResponse>(),
                TotalMoney = oi.TotalPrice ?? 0,
                Status = oi.Status.ToString()
            }).ToList() ?? new List<InvoiceDetailResponse>()
        };
    }


    public Task<PaginatedList<InvoiceResponse>> getAllInvoice(PagingRequestModel pagingRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResponseModel<InvoiceResponse>> getInvoiceById(Guid id)
    {
        var invoice = await _unitOfWork.Repository<Invoice, Guid>()
       .GetWithSpecAsync(new InvoiceSpecification(id));

        if (invoice == null)
            throw new ErrorException(StatusCodes.Status404NotFound,
                ResponseCodeConstants.NOT_FOUND,
                "Không tìm thấy hóa đơn.");
        var response = _mapper.Map<InvoiceResponse>(invoice);
        return new BaseResponseModel<InvoiceResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, response, "Invoice retrived successfully  ");

    }

    public async Task<BaseResponseModel<InvoiceResponse>> getInvoiceByTableId(Guid OrderId)
    {
        var specification = new InvoiceSpecification(OrderId, true);
        var invoices = await _unitOfWork.Repository<Invoice, Guid>().GetWithSpecAsync(specification);
        var response = _mapper.Map<InvoiceResponse>(invoices);


        return new BaseResponseModel<InvoiceResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, response, "Invoice retrived successfully");
    }
}