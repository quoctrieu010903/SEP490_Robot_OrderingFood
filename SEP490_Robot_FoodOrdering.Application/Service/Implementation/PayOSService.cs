using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation;

public class PayOSService: IPayOSService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayOS _payOS;
    private readonly IConfiguration _config;
    private readonly ILogger<PayOSService> _logger;
    
    public PayOSService(IUnitOfWork unitOfWork, PayOS payOS, IConfiguration config, ILogger<PayOSService> logger)
    {
        _unitOfWork = unitOfWork;
        _payOS = payOS;
        _config = config;
        _logger = logger;
    }

    public Task<BaseResponseModel<OrderPaymentResponse>> CreatePaymentLink(Guid orderId)
    {
        var order = await _unitOfWork.Repository<Order, Guid>()
            .GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payment, o => o.Table, o => o.OrderItems);
        if (order == null)
            return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found");

        // Ensure Payment entity
        var isNewPayment = false;
        if (order.Payment == null)
        {
            order.Payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                PaymentMethod = PaymentMethodEnums.PayOS, 
                PaymentStatus = PaymentStatusEnums.Pending,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow
            };
            isNewPayment = true;
        }
        order.Payment.PaymentMethod = PaymentMethodEnums.PayOS; // PaymentMethodEnums.PayOS
        order.Payment.PaymentStatus = PaymentStatusEnums.Pending;
        order.Payment.LastUpdatedTime = DateTime.UtcNow;
        _logger.LogInformation($"CreatePaymentLink create Payment Entity success - orderId {orderId}");
            // Generate unique int order code for PayOS and persist
        var payOsOrderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
        order.Payment.PayOSOrderCode = payOsOrderCode;
            // Persist changes explicitly to avoid EF marking a new Payment as Modified (causing concurrency error)
        if (isNewPayment)
        {
            await _unitOfWork.Repository<Payment, Guid>().AddAsync(order.Payment);
            await _unitOfWork.SaveChangesAsync();
        }    
            // Amount: PayOS expects int (VND)
        var amount = Convert.ToInt32(Math.Round(order.TotalPrice, 0, MidpointRounding.AwayFromZero));
        
        // Description: "Order {Code} – Table {Name}"
        var orderCodeDisplay = order.Code ?? order.Id.ToString();
        var tableName = order.Table?.Name ?? "N/A";
        var description = $"Order {orderCodeDisplay} – Table {tableName}";
        // Items: can combine into one total line
        var items = new List<ItemData>
        {
            new ItemData($"Order {orderCodeDisplay}", 1, amount)
        };
        var returnUrl = _config["Environment:PAYOS_RETURN_URL"] ?? throw new Exception("Missing PAYOS_RETURN_URL");
        var cancelUrl = _config["Environment:PAYOS_CANCEL_URL"] ?? throw new Exception("Missing PAYOS_CANCEL_URL");

        var paymentData = new PaymentData(
            payOsOrderCode,
            amount,
            description,
            items,
            cancelUrl,
            returnUrl
        );

        var created = await _payOS.createPaymentLink(paymentData);

        return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, "PAYMENT_INITIATED", new OrderPaymentResponse
        {
            OrderId = orderId,
            PaymentStatus = PaymentStatusEnums.Pending,
            PaymentUrl = created.checkoutUrl,
            Message = "Redirect to PayOS for payment."
        });
        
    }

    public Task HandleWebhook(WebhookType body)
    {
        var data = _payOS.verifyPaymentWebhookData(body);
        // Tìm payment theo PayOS order code
        var payment = (await _unitOfWork.Repository<Payment, Guid>()
            .GetAsync(x => x.PayOSOrderCode == data.orderCode, includeProperties: p => p.Order, asNoTracking: false))
            .FirstOrDefault();

        if (payment == null)
        {
            _logger.LogWarning("PayOS webhook: payment not found for orderCode {OrderCode}", data.orderCode);
            return;
        }

        var order = payment.Order;
        if (order == null)
        {
            _logger.LogWarning("PayOS webhook: order not found for payment {PaymentId}", payment.Id);
            return;
        }

        // Idempotency: nếu đã Paid thì không làm gì thêm
        if (payment.PaymentStatus == PaymentStatusEnums.Paid)
            return;
        var isSuccess = string.Equals(data.description, "Ma giao dich thu nghiem", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(data.desc, "Ma giao dich thu nghiem", StringComparison.OrdinalIgnoreCase)
                        || data.code == "00" // nếu PayOS trả mã thành công
                        || string.Equals(data.status, "SUCCESS", StringComparison.OrdinalIgnoreCase);

        if (isSuccess)
        {
            payment.PaymentStatus = PaymentStatusEnums.Paid;
            order.PaymentStatus = PaymentStatusEnums.Paid;

            foreach (var item in order.OrderItems)
            {
                item.Status = OrderItemStatus.Completed;
                item.LastUpdatedTime = DateTime.UtcNow;
            }

            if (order.Table != null)
            {
                order.Table.Status = TableEnums.Available;
                order.Table.DeviceId = null;
                order.Table.IsQrLocked = false;
                order.Table.LockedAt = null;
                await _unitOfWork.Repository<Table, Guid>().UpdateAsync(order.Table);
            }

            order.Status = OrderStatus.Completed;
        }
        else
        {
            payment.PaymentStatus = PaymentStatusEnums.Failed;
            order.PaymentStatus = PaymentStatusEnums.Failed;
        }                
        payment.LastUpdatedTime = DateTime.UtcNow;
        await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
        await _unitOfWork.Repository<Payment, Guid>().UpdateAsync(payment);
        await _unitOfWork.SaveChangesAsync();
    }
}