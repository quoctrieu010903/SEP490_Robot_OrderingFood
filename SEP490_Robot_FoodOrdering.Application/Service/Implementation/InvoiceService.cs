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
    private readonly ISettingsService _settingsService;

    public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper, IUtilsService utilsService, ISettingsService settingsService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _utilsService = utilsService;
        _settingsService = settingsService;
    }

    public async Task<InvoiceResponse> CreateInvoice(InvoiceCreatRequest request)
    {
        var existedOrder = await _unitOfWork.Repository<Order, Guid>()
            .GetWithSpecAsync(new OrderSpecification(request.OrderId, true));

        if (existedOrder == null)
            throw new ErrorException(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found");

        var existedInvoice = await _unitOfWork.Repository<Invoice, Guid>()
            .GetWithSpecAsync(new BaseSpecification<Invoice>(x => x.OrderId == existedOrder.Id));

        if (existedInvoice != null)
        {
             var rName = (await _settingsService.GetByKeyAsync(SystemSettingKeys.RestaurantName))?.Data?.Value ?? "Unknown Restaurant";
             return BuildInvoiceResponse(existedInvoice, existedOrder, rName);
        }

        // ✅ Tạo ID trước để FK detail luôn đúng
        var invoiceId = Guid.NewGuid();

        var invoice = new Invoice
        {
            Id = invoiceId, // ✅ QUAN TRỌNG
            OrderId = existedOrder.Id,
            TableId = existedOrder.TableId ?? request.TableId,
            CustomerId = request.CustomerId,
            InvoiceCode = _utilsService.GenerateCode("HD", 6),
            CreatedTime = DateTime.UtcNow,
            TotalMoney = existedOrder.TotalPrice,
            PaymentMethod = existedOrder.paymentMethod,
            Status = existedOrder.PaymentStatus,
            Details = new List<InvoiceDetail>()
        };

        var orderStatus = existedOrder.Status;

        if (existedOrder.OrderItems != null)
        {
            foreach (var oi in existedOrder.OrderItems)
            {
                invoice.Details.Add(new InvoiceDetail
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoiceId,      // ✅ QUAN TRỌNG: set FK trực tiếp
                                                // Invoice = invoice,        // nếu entity nav của bạn là Invoice (singular) thì dùng dòng này
                                                // Invoices = invoice,       // nếu bạn đặt tên nav là Invoices thì giữ như vậy, nhưng vẫn nên set InvoiceId
                    OrderItemId = oi.Id,
                    TotalMoney = oi.TotalPrice ?? 0,
                    Status = orderStatus
                });
            }
        }

        await _unitOfWork.Repository<Invoice, Guid>().AddAsync(invoice);
        // ❌ Không SaveChanges ở đây

        var restaurantName = (await _settingsService.GetByKeyAsync(SystemSettingKeys.RestaurantName))?.Data?.Value ?? "Unknown Restaurant";
        return BuildInvoiceResponse(invoice, existedOrder, restaurantName);
    }

    private InvoiceResponse BuildInvoiceResponse(Invoice invoice, Order existedOrder, string restaurantName)
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
            PaymentStatus = existedOrder.PaymentStatus.ToString(),
            TotalAmount = existedOrder.TotalPrice,
            Discount = 0,
            FinalAmount = existedOrder.TotalPrice,
            RestaurantName = restaurantName,
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
        var restaurantName = (await _settingsService.GetByKeyAsync(SystemSettingKeys.RestaurantName))?.Data?.Value ?? "Unknown Restaurant";
        var restaurantAddress = await _settingsService.GetByKeyAsync(SystemSettingKeys.RestaurantAddress);
        var restaurantPhoneNumber = await _settingsService.GetByKeyAsync(SystemSettingKeys.RestaurantPhone);
        var response = _mapper.Map<InvoiceResponse>(invoice);
        response.RestaurantName = restaurantName;
        response.RestaurantPhone = restaurantPhoneNumber.Data.Value;
        response.RestaurantAddress = restaurantAddress.Data.Value;
        return new BaseResponseModel<InvoiceResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, response, "Invoice retrived successfully  ");

    }

    public async Task<BaseResponseModel<InvoiceResponse>> getInvoiceByTableId(Guid OrderId)
    {
        var specification = new InvoiceSpecification(OrderId, true);
        var invoices = await _unitOfWork.Repository<Invoice, Guid>().GetWithSpecAsync(specification);
        var restaurantName = (await _settingsService.GetByKeyAsync(SystemSettingKeys.RestaurantName))?.Data?.Value ?? "Unknown Restaurant";
        var response = _mapper.Map<InvoiceResponse>(invoices);
        response.RestaurantName = restaurantName;


        return new BaseResponseModel<InvoiceResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, response, "Invoice retrived successfully");
    }

    public async Task<BaseResponseModel<LatestInvoiceByPhoneResponse>> GetLatestInvoiceByPhoneAsync(string phone)
    {
        var restaurantName = await _settingsService.GetByKeyAsync(SystemSettingKeys.RestaurantName);
        var restaurantAddress = await _settingsService.GetByKeyAsync(SystemSettingKeys.RestaurantAddress);
        var restaurantPhoneNumber = await _settingsService.GetByKeyAsync(SystemSettingKeys.RestaurantPhone);
        if (string.IsNullOrWhiteSpace(phone))
            throw new ErrorException(StatusCodes.Status400BadRequest,
                ResponseCodeConstants.VALIDATION_ERROR,
                "Vui lòng nhập số điện thoại");

        var normalizedPhone = NormalizeVnPhone(phone);

        if (string.IsNullOrWhiteSpace(normalizedPhone) || normalizedPhone.Length < 9 || normalizedPhone.Length > 11)
            throw new ErrorException(StatusCodes.Status400BadRequest,
                ResponseCodeConstants.VALIDATION_ERROR,
                "SĐT không hợp lệ");

        // 1) Find customer by phone
        var customer = await _unitOfWork.Repository<Customer, Guid>()
            .GetWithSpecAsync(new BaseSpecification<Customer>(c => c.PhoneNumber == normalizedPhone));

        if (customer == null)
            throw new ErrorException(StatusCodes.Status404NotFound,
                ResponseCodeConstants.NOT_FOUND,
                "Không tìm thấy khách hàng theo SĐT");

        // 2) Get latest invoice by customerId
        var invoiceSpec = new BaseSpecification<Invoice>(i =>
            i.CustomerId == customer.Id
        // ✅ Nếu chỉ cho xuất hoá đơn đỏ khi đã thanh toán:
        // && i.Status == PaymentStatusEnums.Paid
        );

        //invoiceSpec.AddOrderByDescending(i => i.CreatedTime);
        //invoiceSpec.ApplyPaging(0, 1); // lấy 1 record mới nhất

        var invoices = await _unitOfWork.Repository<Invoice, Guid>()
            .GetAllWithSpecAsync(invoiceSpec);

        var latestInvoice = invoices.FirstOrDefault();

        if (latestInvoice == null)
            throw new ErrorException(StatusCodes.Status404NotFound,
                ResponseCodeConstants.NOT_FOUND,
                "Khách hàng chưa có hoá đơn");

        // 3) Load order full để build response (items/topping/table...)
        var existedOrder = await _unitOfWork.Repository<Order, Guid>()
            .GetWithSpecAsync(new OrderSpecification(latestInvoice.OrderId, true));

        if (existedOrder == null)
            throw new ErrorException(StatusCodes.Status404NotFound,
                ResponseCodeConstants.NOT_FOUND,
                "Order của hoá đơn không tồn tại");

     
        var invoiceResponse = BuildInvoiceResponse(latestInvoice, existedOrder, restaurantName.Data.Value);

        var data = new LatestInvoiceByPhoneResponse
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            RestaurantName = restaurantName.Data.Value,
            RestaurantAddress = restaurantAddress.Data.Value,
            RestaurantPhone =restaurantPhoneNumber.Data.Value,
            PhoneNumber = normalizedPhone,
            TotalPoins = customer.TotalPoints,
            Invoice = invoiceResponse
        };

        return new BaseResponseModel<LatestInvoiceByPhoneResponse>(
            StatusCodes.Status200OK,
            ResponseCodeConstants.SUCCESS,
            data,
            "Lấy hoá đơn mới nhất theo SĐT thành công"
        );
    }

    private static string NormalizeVnPhone(string raw)
    {
        var p = raw.Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace(".", "");

        if (p.StartsWith("+84")) p = "0" + p.Substring(3);
        if (p.StartsWith("84") && p.Length >= 10) p = "0" + p.Substring(2);

        return p;
    }
}