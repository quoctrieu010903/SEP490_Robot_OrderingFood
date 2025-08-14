using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;


namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IPaymentService
    {
        Task<BaseResponseModel<OrderPaymentResponse>> CreateVNPayPaymentUrl(Guid orderId, string moneyUnit, string paymentContent);
        Task<BaseResponseModel<OrderPaymentResponse>> HandleVNPayReturn(IQueryCollection queryCollection);
        Task<BaseResponseModel<OrderPaymentResponse>> HandleVNPayIpn(IQueryCollection queryCollection);
    }
}
