using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using SEP490_Robot_FoodOrdering.Application.Abstractions.ServerEndPoint;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation;

public class PayOSService: IPayOSService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayOS _payOS;
    private readonly IConfiguration _config;
    private readonly ILogger<PayOSService> _logger;
    private readonly IServerEndpointService _serverEndpointService;

    public PayOSService(IUnitOfWork unitOfWork, PayOS payOS, IConfiguration config, ILogger<PayOSService> logger, IServerEndpointService serverEndpointService )
    {
        _unitOfWork = unitOfWork;
        _payOS = payOS;
        _config = config;
        _logger = logger;
        _serverEndpointService = serverEndpointService;
    }

    public async Task<BaseResponseModel<OrderPaymentResponse>> CreatePaymentLink(Guid orderId, bool isCustomer)
    {
        // 1️⃣ Lấy order cùng các payment, item, table
        var order = await _unitOfWork.Repository<Order, Guid>()
            .GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payments, o => o.Table, o => o.OrderItems);

        if (order == null)
            return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found");

        // 2️⃣ Xác định danh sách món chưa thanh toán
        var unpaidItems = order.OrderItems
            .Where(oi => oi.Status != OrderItemStatus.Cancelled && oi.PaymentStatus != PaymentStatusEnums.Paid)
            .ToList();

        if (!unpaidItems.Any())
        {
            return new BaseResponseModel<OrderPaymentResponse>(
                StatusCodes.Status400BadRequest,
                "NO_UNPAID_ITEMS",
                "All items in this order have already been paid."
            );
        }

        // 3️⃣ Tính tổng tiền cần thanh toán
        var unpaidAmount = unpaidItems.Sum(oi => oi.TotalPrice) ?? 0;
        var amount = Convert.ToInt32(Math.Round(unpaidAmount, 0, MidpointRounding.AwayFromZero));

        // 4️⃣ Sinh mã PayOSOrderCode duy nhất
        var payOsOrderCode = int.Parse(DateTimeOffset.UtcNow.ToString("ffffff"));
        var shortLabel = $"ORD {payOsOrderCode}";
        var description = shortLabel;

        // 5️⃣ Tạo Payment mới (không ghi đè Payment cũ)
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PaymentMethod = PaymentMethodEnums.PayOS,
            PaymentStatus = PaymentStatusEnums.Pending,
            PayOSOrderCode = payOsOrderCode,
            CreatedTime = DateTime.UtcNow,
            LastUpdatedTime = DateTime.UtcNow
        };


        await _unitOfWork.Repository<Payment, Guid>().AddAsync(payment);
        await _unitOfWork.SaveChangesAsync(); // Lưu trước để có mã PayOSOrderCode

        // 6️⃣ Gọi PayOS để tạo link thanh toán
        var items = new List<ItemData> { new ItemData(shortLabel, 1, amount) };

        // var returnUrl = isCustomer
        //     ? _config["Environment:PAYOS_RETURN_URL"]
        //     : _config["Environment:PAYOS_MODERATOR_RETURN_URL"];

        // var cancelUrl = isCustomer
        //     ? _config["Environment:PAYOS_CANCEL_URL"]
        //     : _config["Environment:PAYOS_MODERATOR_CANCEL_URL"];
        var returnUrl = _serverEndpointService.GetBackendUrl() + $"/PayOS/success/{orderId}?isCustomer={isCustomer}";

        var cancelUrl = _serverEndpointService.GetBackendUrl() + $"/PayOS/cancel/{orderId}?isCustomer={isCustomer}";

        var paymentData = new PaymentData(
            payOsOrderCode,
            amount,
            description,
            items,
            cancelUrl!,
            returnUrl!
        );

        var created = await _payOS.createPaymentLink(paymentData);

        // foreach (var item in unpaidItems)
        // {
        //     item.PaymentStatus = PaymentStatusEnums.Paid;
        //     await _unitOfWork.Repository<OrderItem, Guid>().UpdateAsync(item);
        // }
        order.PaymentStatus = PaymentStatusEnums.Pending;
        payment.PaymentStatus = PaymentStatusEnums.Pending; 
        payment.LastUpdatedTime = DateTime.UtcNow;

        _unitOfWork.Repository<Payment, Guid>().Update(payment);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation($"[CreatePaymentLink] Success | OrderId: {order.Id}, PaymentId: {payment.Id}, Amount: {amount}");
        
        // _logger.LogInformation($"Sync payment status  OrderId: {order.Id} - start");
        // await SyncOrderPaymentStatus(orderId);
        // _logger.LogInformation($"Sync payment status  OrderId: {order.Id} - end");
        // 8️⃣ Trả về thông tin cho frontend
        return new BaseResponseModel<OrderPaymentResponse>(
            StatusCodes.Status200OK,
            "PAYMENT_INITIATED",
            new OrderPaymentResponse
            {
                OrderId = orderId,
                PaymentStatus = payment.PaymentStatus,
                PaymentUrl = created.checkoutUrl,
                Message = "Redirect to PayOS for payment."
            }
        );
    }

    public async Task HandleWebhook(WebhookType body)
    {
        _logger.LogInformation("PayOS webhook: start handling");
        WebhookData data;
        try
        {
            data =  _payOS.verifyPaymentWebhookData(body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayOS webhook: signature verification failed");
            return; // cannot trust payload
        }
        _logger.LogInformation("PayOS webhook: verified payload = {Payload}", JsonSerializer.Serialize(data));

        // Tìm payment theo PayOS order code, include Order
        var payment = await _unitOfWork.Repository<Payment, Guid>()
            .GetByIdWithIncludeAsync(p => p.PayOSOrderCode == data.orderCode, true, p => p.Order);

        if (payment == null)
        {
            _logger.LogInformation("PayOS webhook: payment not found for orderCode {OrderCode}", data.orderCode);
            return;
        }

        // Nạp Order với Items và Table
        var order = await _unitOfWork.Repository<Order, Guid>()
            .GetByIdWithIncludeAsync(x => x.Id == payment.OrderId, true, o => o.OrderItems, o => o.Table);
        if (order == null)
        {
            _logger.LogInformation("PayOS webhook: order not found for payment {PaymentId}", payment.Id);
            return;
        }

        // Idempotency: if we've already marked Paid at either level, do nothing
        if (payment.PaymentStatus == PaymentStatusEnums.Paid || order.PaymentStatus == PaymentStatusEnums.Paid)
        {
            _logger.LogInformation("PayOS webhook: skip, already paid for orderId {OrderId} (orderCode {OrderCode})", order.Id, data.orderCode);
            return;
        }

        // Determine payment success status from verified payload (code "00" indicates success)
        var isSuccess = string.Equals(data.code, "00", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(body.code, "00", StringComparison.OrdinalIgnoreCase);

        if (isSuccess)
        {
            order.PaymentStatus = PaymentStatusEnums.Paid;
            payment.PaymentStatus = PaymentStatusEnums.Paid;
            payment.PaymentMethod = PaymentMethodEnums.PayOS;
        }
        else
        {
            _logger.LogInformation(
                "PayOS webhook: non-success code for orderId {OrderId} (orderCode {OrderCode}) - data.code={DataCode}, body.code={BodyCode}, data.desc={DataDesc}, body.desc={BodyDesc}, data.description={DataDescription}",
                order.Id,
                data.orderCode,
                data.code,
                body.code,
                data.desc,
                body.desc,
                data.description
            );
            order.PaymentStatus = PaymentStatusEnums.Failed;
            payment.PaymentStatus = PaymentStatusEnums.Failed;
            payment.PaymentMethod = PaymentMethodEnums.PayOS;
        }

        // ensure order reflects PayOS as the method used
        order.paymentMethod = PaymentMethodEnums.PayOS;

        await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
        await _unitOfWork.Repository<Payment, Guid>().UpdateAsync(payment);
        await _unitOfWork.SaveChangesAsync();
    }

    // Manual sync in case webhook missed or for one-off fix
    public async Task<BaseResponseModel<OrderPaymentResponse>> SyncOrderPaymentStatus(Guid orderId)
    {
        // 1️⃣ Lấy order kèm danh sách payment, order item và table
        var order = await _unitOfWork.Repository<Order, Guid>()
            .GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payments, o => o.OrderItems, o => o.Table);

        if (order == null)
            return new BaseResponseModel<OrderPaymentResponse>(
                StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found");

        
        
        // 2️⃣ Nếu chưa có Payment nào thì coi như Pending
        if (order.Payments == null || !order.Payments.Any())
        {
            order.PaymentStatus = PaymentStatusEnums.Pending;
        }
        else
        {
            // ✅ Tất cả thanh toán thành công → Paid
            if (order.Payments.All(p => p.PaymentStatus == PaymentStatusEnums.Paid))
            {
                order.PaymentStatus = PaymentStatusEnums.Paid;
            }
            // ✅ Tất cả thanh toán thất bại → Failed
            else if (order.Payments.All(p => p.PaymentStatus == PaymentStatusEnums.Failed))
            {
                order.PaymentStatus = PaymentStatusEnums.Failed;
            }
            // ✅ Có ít nhất một Payment chưa xử lý → Pending
            else
            {
                order.PaymentStatus = PaymentStatusEnums.Pending;
            }
        }

        // 3️⃣ Cập nhật lại các OrderItem theo trạng thái tổng thể của Order
        foreach (var item in order.OrderItems)
        {
            item.PaymentStatus = order.PaymentStatus;
            await _unitOfWork.Repository<OrderItem, Guid>().UpdateAsync(item);
        }


        // 4️⃣ Cập nhật Order
        order.LastUpdatedTime = DateTime.UtcNow;
        await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
        var session = await _unitOfWork.Repository<TableSession, Guid>()
            .GetByIdWithIncludeAsync(ts => ts.Id == order.TableSessionId, false, ts => ts.Table);

       
        await _unitOfWork.SaveChangesAsync();

        // 5️⃣ Trả kết quả đồng bộ
        return new BaseResponseModel<OrderPaymentResponse>(
            StatusCodes.Status200OK,
            "SYNCED",
            new OrderPaymentResponse
            {
                OrderId = orderId,
                PaymentStatus = order.PaymentStatus,
                Message = "Order payment status synchronized successfully"
            });
    }
    
    
    // Cancel api if the payment not success.
    public async Task<BaseResponseModel<OrderPaymentResponse>> CancelOrderPaymentStatus(Guid orderId,  bool isCustomer)
    {
        // get order by order id including payments, order items, tables
        var order = await _unitOfWork.Repository<Order, Guid>()
            .GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payments, o => o.OrderItems, o => o.Table);

        if (order == null)
            return new BaseResponseModel<OrderPaymentResponse>(
                StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found");
        // get check the order items, if the order item paid. still get the same. 

        var hasAnySuccessfulPayment = order.Payments != null && order.Payments.Any(p => p.PaymentStatus == PaymentStatusEnums.Paid);
        if (!hasAnySuccessfulPayment)
            order.PaymentStatus = PaymentStatusEnums.Pending;
        
        // Update lại order.
        order.LastUpdatedTime = DateTime.UtcNow;
        var tableId = order.TableId;
        await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        //var frontendCancelURL = _serverEndpointService.GetFrontendUrl() + "payment/cancel";
        
        var cancelUrl = isCustomer
        ? _config["Environment:PAYOS_CANCEL_URL"]
        : _config["Environment:PAYOS_MODERATOR_CANCEL_URL"];
        var finalReturnUrl = cancelUrl;
        if (tableId.HasValue)
        {
             finalReturnUrl = cancelUrl + $"/{tableId}";
            _logger.LogInformation($"Final payment return: {finalReturnUrl}");
        }
        return new BaseResponseModel<OrderPaymentResponse>(
            StatusCodes.Status200OK,
            "CANCELLED",
            new OrderPaymentResponse
            {
                OrderId = orderId,
                //PaymentUrl = frontendCancelURL,
                PaymentUrl = finalReturnUrl,
                PaymentStatus = order.PaymentStatus,
                Message = "payment status cancelled."
            });
    }

    public async Task<BaseResponseModel<OrderPaymentResponse>> CompleteOrderPaymentStatus(Guid orderId, bool isCustomer)
    {
       // var orderSuccessResponse = await SyncOrderPaymentStatus(orderId);
       
       var order = await _unitOfWork.Repository<Order, Guid>()
           .GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payments, o => o.OrderItems, o => o.Table);
       
       if (order == null)
           return new BaseResponseModel<OrderPaymentResponse>(
               StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found");


       // Xác định món chưa thanh toán
       var unpaidItems = order.OrderItems
           .Where(oi => oi.Status != OrderItemStatus.Cancelled && oi.PaymentStatus != PaymentStatusEnums.Paid)
           .ToList();
       foreach (var item in unpaidItems)
       {
           item.PaymentStatus = PaymentStatusEnums.Paid;
           await _unitOfWork.Repository<OrderItem, Guid>().UpdateAsync(item);
       }

       // update order payment status
       order.PaymentStatus= PaymentStatusEnums.Paid;
       order.LastUpdatedTime = DateTime.UtcNow;
       var tableId = order.TableId;
       await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
       await _unitOfWork.SaveChangesAsync();
       
       
       var returnUrl = isCustomer
            ? _config["Environment:PAYOS_RETURN_URL"]
            : _config["Environment:PAYOS_MODERATOR_RETURN_URL"];
       var finalReturnUrl = returnUrl;
       if (tableId.HasValue)
       {
           finalReturnUrl = returnUrl + $"/{tableId}";
           _logger.LogInformation($"Final payment return: {finalReturnUrl}");
       }
       
        // update return url
        // if (orderSuccessResponse.Data != null)
        // {
        //     orderSuccessResponse.Data.PaymentUrl = returnUrl;
        // }
        // return orderSuccessResponse;
        
        return new BaseResponseModel<OrderPaymentResponse>(
            StatusCodes.Status200OK,
            "SUCCESSS",
            new OrderPaymentResponse
            {
                OrderId = orderId,
                PaymentStatus = order.PaymentStatus,
                PaymentUrl = finalReturnUrl,
                Message = "Order payment status synchronized successfully"
            });
    }
}