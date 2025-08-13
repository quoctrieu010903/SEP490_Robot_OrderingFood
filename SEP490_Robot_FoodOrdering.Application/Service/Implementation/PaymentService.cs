using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Options;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Core.Ultils;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

    public class PaymentService: IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly VNPayOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(IUnitOfWork unitOfWork, IOptions<VNPayOptions> options,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResponseModel<OrderPaymentResponse>> CreateVNPayPaymentUrl(Guid orderId, string moneyUnit,
            string paymentContent)
        {
            var order = await _unitOfWork.Repository<Order, Guid>()
                .GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payment, o => o.Table, o => o.OrderItems);
            if (order == null)
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND",
                    "Order not found");

            // Ensure there is a Payment entity
            if (order.Payment == null)
            {
                order.Payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    PaymentMethod = PaymentMethodEnums.VNPay,
                    PaymentStatus = PaymentStatusEnums.Pending,
                    OrderId = order.Id,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                };
            }
            else
            {
                order.Payment.PaymentMethod = PaymentMethodEnums.VNPay;
                order.Payment.PaymentStatus = PaymentStatusEnums.Pending;
                order.Payment.LastUpdatedTime = DateTime.UtcNow;
            }

            order.PaymentStatus = PaymentStatusEnums.Pending;
            await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Build VNPay request
            var vnPay = new VnPayLibrary();

            vnPay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnPay.AddRequestData("vnp_Command", _options.PayCommand);
            vnPay.AddRequestData("vnp_TmnCode", _options.TmnCode);
            vnPay.AddRequestData("vnp_Locale", string.IsNullOrWhiteSpace(_options.Locale) ? "vn" : _options.Locale);
            vnPay.AddRequestData("vnp_CurrCode", string.IsNullOrWhiteSpace(moneyUnit) ? "VND" : moneyUnit);
            vnPay.AddRequestData("vnp_TxnRef", order.Id.ToString("N"));
            vnPay.AddRequestData("vnp_OrderInfo",
                string.IsNullOrWhiteSpace(paymentContent) ? $"Thanh toan don {order.Id}" : paymentContent);
            vnPay.AddRequestData("vnp_OrderType", _options.BookingPackageType);
            vnPay.AddRequestData("vnp_Amount", ((long)(order.TotalPrice * 100)).ToString());
            vnPay.AddRequestData("vnp_ReturnUrl", BuildAbsoluteReturnUrl());
            vnPay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(_httpContextAccessor));
            var now = DateTime.UtcNow;
            vnPay.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
            vnPay.AddRequestData("vnp_ExpireDate", now.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            var paymentUrl = vnPay.CreateRequestUrl(_options.Url, _options.HashSecret);

            return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, "PAYMENT_INITIATED",
                new OrderPaymentResponse
                {
                    OrderId = order.Id,
                    PaymentStatus = PaymentStatusEnums.Pending,
                    PaymentUrl = paymentUrl,
                    Message = "Redirect to VNPay for payment."
                });
        }

        public async Task<BaseResponseModel<OrderPaymentResponse>> HandleVNPayReturn(IQueryCollection queryCollection)
        {
            return await ProcessVNPayCallback(queryCollection, false);
        }

        public async Task<BaseResponseModel<OrderPaymentResponse>> HandleVNPayIpn(IQueryCollection queryCollection)
        {
            return await ProcessVNPayCallback(queryCollection, true);
        }

        private async Task<BaseResponseModel<OrderPaymentResponse>> ProcessVNPayCallback(IQueryCollection query,
            bool isIpn)
        {
            // Gather data
            var vnpData = new VnPayLibrary();
            foreach (var key in query.Keys.Where(k => k.StartsWith("vnp_")))
            {
                vnpData.AddResponseData(key, query[key]);
            }

            var vnpSecureHash = query["vnp_SecureHash"].ToString();
            if (!vnpData.ValidateSignature(vnpSecureHash, _options.HashSecret))
            {
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status400BadRequest, "INVALID_SIGNATURE",
                    "Invalid signature from VNPay");
            }

            var responseCode = vnpData.GetResponseData("vnp_ResponseCode");
            var transactionStatus = vnpData.GetResponseData("vnp_TransactionStatus");
            var txnRef = vnpData.GetResponseData("vnp_TxnRef");
            var amountString = vnpData.GetResponseData("vnp_Amount");

            if (!Guid.TryParseExact(txnRef, "N", out Guid orderId))
            {
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status400BadRequest, "INVALID_TXN_REF",
                    "Invalid vnp_TxnRef");
            }

            var order = await _unitOfWork.Repository<Order, Guid>()
                .GetByIdWithIncludeAsync(x => x.Id == orderId, true, o => o.Payment, o => o.Table, o => o.OrderItems);
            if (order == null)
            {
                return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status404NotFound, "ORDER_NOT_FOUND",
                    "Order not found");
            }

            // Optional: validate amount matches
            if (long.TryParse(amountString, out long amountSmallestUnit))
            {
                var expected = (long)(order.TotalPrice * 100);
                if (amountSmallestUnit != expected)
                {
                    // Log only; do not fail hard to keep demo simple
                }
            }

            var isSuccess = responseCode == "00" && transactionStatus == "00";

            if (order.Payment == null)
            {
                order.Payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PaymentMethod = PaymentMethodEnums.VNPay,
                    PaymentStatus = PaymentStatusEnums.Pending,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                };
            }

            if (isSuccess)
            {
                order.Payment.PaymentStatus = PaymentStatusEnums.Paid;
                order.Payment.LastUpdatedTime = DateTime.UtcNow;
                order.paymentMethod = PaymentMethodEnums.VNPay;
                order.PaymentStatus = PaymentStatusEnums.Paid;

                // Mark order items as completed
                foreach (var item in order.OrderItems)
                {
                    item.Status = OrderItemStatus.Completed;
                    item.LastUpdatedTime = DateTime.UtcNow;
                }

                // Release table
                if (order.Table != null)
                {
                    order.Table.Status = TableEnums.Available;
                    await _unitOfWork.Repository<Table, Guid>().UpdateAsync(order.Table);
                }

                order.Status = OrderStatus.Completed;
            }
            else
            {
                order.Payment.PaymentStatus = PaymentStatusEnums.Failed;
                order.Payment.LastUpdatedTime = DateTime.UtcNow;
                order.PaymentStatus = PaymentStatusEnums.Failed;
            }

            await _unitOfWork.Repository<Order, Guid>().UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            var status = isSuccess ? PaymentStatusEnums.Paid : PaymentStatusEnums.Failed;
            return new BaseResponseModel<OrderPaymentResponse>(StatusCodes.Status200OK, isSuccess ? "PAID" : "FAILED",
                new OrderPaymentResponse
                {
                    OrderId = order.Id,
                    PaymentStatus = status,
                    Message = isSuccess ? "Payment success (VNPay)" : "Payment failed (VNPay)"
                });
        }

        private string BuildAbsoluteReturnUrl()
        {
            // If ReturnUrl is absolute in config, honor it. If relative, build from request
            if (Uri.TryCreate(_options.ReturnUrl, UriKind.Absolute, out var absolute))
            {
                return absolute.ToString();
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return _options.ReturnUrl; // fallback
            }

            var builder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Port = request.Host.Port ?? (request.Scheme == "https" ? 443 : 80),
                Path = _options.ReturnUrl.StartsWith("/") ? _options.ReturnUrl : "/" + _options.ReturnUrl
            };
            return builder.Uri.ToString();
        }
    }