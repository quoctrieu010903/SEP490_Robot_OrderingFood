using System.Net.NetworkInformation;
using AutoMapper;
using Microsoft.AspNetCore.Http;
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

    public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceResponse> CreateInvoice(InvoiceCreatRequest request)
    {
        // 1️⃣ Lấy order từ DB kèm theo OrderItems và Table
        var existedOrder = await _unitOfWork.Repository<Order, Guid>()
            .GetByIdWithIncludeAsync(
                o => o.Id == request.OrderId,
                true,
                o => o.OrderItems,
                o => o.OrderItems!.Select(oi => oi.Product),
                o => o.Table
            );

        if (existedOrder == null)
        {
            throw new ErrorException(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found");
        }

        // 2️⃣ Kiểm tra order đã thanh toán chưa
        if (existedOrder.PaymentStatus != PaymentStatusEnums.Paid)
        {
            throw new ErrorException(StatusCodes.Status400BadRequest, "ORDER_NOT_PAID", "Order is not paid yet");
        }

        // 3️⃣ Tạo mới Invoice từ Order
        var invoice = new Invoice()
        {
            OrderId = existedOrder.Id,
            TableId = request.TableId,   // nếu không truyền TableId thì lấy từ order
            CreatedTime = DateTime.UtcNow,
            TotalMoney = existedOrder.TotalPrice,               // tùy tên property trong model
            PaymentMethod = existedOrder.paymentMethod,          // chú ý tên property đúng
            Status = existedOrder.PaymentStatus,

            // 4️⃣ Tạo danh sách InvoiceDetails
            Details = existedOrder.OrderItems?.Select(oi => new InvoiceDetail()
            {
                OrderItemId = oi.Id,
                TotalMoney = oi.TotalPrice ?? 0,
                Status = oi.Order.Status,

            }).ToList() ?? new List<InvoiceDetail>()
        };

        // 5️⃣ Lưu vào DB
        await _unitOfWork.Repository<Invoice, Guid>().AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        var response = new InvoiceResponse
        {
            Id = invoice.Id,
            OrderId = invoice.OrderId,
            TableId = invoice.TableId,
            TableName = existedOrder.Table?.Name,
            CreatedTime = invoice.CreatedTime,
            PaymentMethod = invoice.PaymentMethod.ToString(),
            TotalAmount = invoice.TotalMoney,
            Discount = 0,
            FinalAmount = invoice.TotalMoney,
            CashierName = "Moderator Manager", // hoặc lấy từ User hiện tại
            Details = invoice.Details.Select(d => new InvoiceDetailResponse
            {
                OrderItemId = d.OrderItemId,
                ProductName = d.OrderItem?.Product.Name,
                UnitPrice = d.OrderItem?.Price ?? 0,
                toppings = d.OrderItem?.OrderItemTopping?.Select(oit => new ToppingResponse
                {
                    Id = oit.Topping.Id,
                    Name = oit.Topping.Name,
                    Price = oit.Topping.Price
                }).ToList() ?? new List<ToppingResponse>(),
                TotalMoney = d.OrderItem?.TotalPrice ?? 0,
                Status = d.Status.ToString()
            }).ToList()
        };



        return response;
    }


    public Task<PaginatedList<InvoiceResponse>> getAllInvoice(PagingRequestModel pagingRequest)
    {
        throw new NotImplementedException();
    }

    public Task<InvoiceResponse> getInvoiceById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResponseModel<InvoiceResponse>> getInvoiceByTableId(Guid OrderId, PagingRequestModel pagingRequest)
    {
        var specification = new InvoiceSpecification(OrderId, pagingRequest.PageNumber, pagingRequest.PageSize);
        var invoices = await _unitOfWork.Repository<Invoice, Guid>().GetWithSpecAsync(specification);
        var response = _mapper.Map<InvoiceResponse>(invoices);


        return new BaseResponseModel<InvoiceResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, response, "Invoice retrived successfully");
    }
}