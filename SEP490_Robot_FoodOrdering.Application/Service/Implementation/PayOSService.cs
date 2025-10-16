using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
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
    
    public PayOSService(IUnitOfWork unitOfWork, PayOS payOS, IConfiguration config, ILogger<PayOSService> logger)
    {
        _unitOfWork = unitOfWork;
        _payOS = payOS;
        _config = config;
        _logger = logger;
    }

    public async Task<BaseResponseModel<OrderPaymentResponse>> CreatePaymentLink(Guid orderId)
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
        order.Payment.PaymentMethod = PaymentMethodEnums.PayOS;
        order.Payment.PaymentStatus = PaymentStatusEnums.Pending;
        // reflect gateway selection at order level
        order.paymentMethod = PaymentMethodEnums.PayOS;
        order.Payment.LastUpdatedTime = DateTime.UtcNow;
        _logger.LogInformation($"CreatePaymentLink create Payment Entity success - orderId {orderId}");
            // Generate unique int order code for PayOS and persist
        var payOsOrderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
        order.Payment.PayOSOrderCode = payOsOrderCode;
            // Persist changes explicitly to avoid EF marking a new Payment as Modified (causing concurrency error)
        if (isNewPayment)
        {
            await _unitOfWork.Repository<Payment, Guid>().AddAsync(order.Payment);
        }    
            // Amount: PayOS expects int (VND)
        var amount = Convert.ToInt32(Math.Round(order.TotalPrice, 0, MidpointRounding.AwayFromZero));
        
        // Description: "ORD payOsOrderCode". Eg: "ORD 841803"
        // Description must be <= 25 chars per PayOS; use short code
        var shortLabel = $"ORD {payOsOrderCode}"; // e.g., "ORD 123456"
        var description = shortLabel;
        // Items: combine into one total line with short label
        var items = new List<ItemData>
        {
            new ItemData(shortLabel, 1, amount)
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

        // Persist after creating link to ensure PayOSOrderCode saved
        await _unitOfWork.SaveChangesAsync();

        return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, "PAYMENT_INITIATED", new OrderPaymentResponse
        {
            OrderId = orderId,
            PaymentStatus = PaymentStatusEnums.Pending,
            PaymentUrl = created.checkoutUrl,
            Message = "Redirect to PayOS for payment."
        });
        
    }

    public async Task HandleWebhook(WebhookType body)
    {
        var data = _payOS.verifyPaymentWebhookData(body);

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

        // Idempotency
        if (payment.PaymentStatus == PaymentStatusEnums.Paid)
            return;

        var isSuccess = string.Equals(data.description, "Ma giao dich thu nghiem", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(data.desc, "Ma giao dich thu nghiem", StringComparison.OrdinalIgnoreCase)
                        || data.code == "00";

        if (isSuccess)
        {
            order.PaymentStatus = PaymentStatusEnums.Paid;
        }
        else
        {
            order.PaymentStatus = PaymentStatusEnums.Failed;
        }

        // ensure order reflects PayOS as the method used
        order.paymentMethod = PaymentMethodEnums.PayOS;

        await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
    }

    // Manual sync in case webhook missed or for one-off fix
    public async Task<BaseResponseModel<OrderPaymentResponse>> SyncOrderPaymentStatus(Guid orderId)
    {
        var order = await _unitOfWork.Repository<Order, Guid>()
            .GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payment, o => o.OrderItems, o => o.Table);
        if (order == null)
            return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND", "Order not found");

        var payment = order.Payment;
        if (payment == null)
            return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status404NotFound, "PAYMENT_NOT_FOUND", "Payment record not found");

        // Align only order.PaymentStatus with latest payment status
        if (payment.PaymentStatus == PaymentStatusEnums.Paid)
        {
            order.PaymentStatus = PaymentStatusEnums.Paid;
        }
        else if (payment.PaymentStatus == PaymentStatusEnums.Failed)
        {
            order.PaymentStatus = PaymentStatusEnums.Failed;
        }
        else
        {
            order.PaymentStatus = PaymentStatusEnums.Pending;
        }

        // reflect payment method from Payment
        order.paymentMethod = payment.PaymentMethod;

        await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, "SYNCED", new OrderPaymentResponse
        {
            OrderId = orderId,
            PaymentStatus = order.PaymentStatus,
            Message = "Order payment status synchronized"
        });
    }
}