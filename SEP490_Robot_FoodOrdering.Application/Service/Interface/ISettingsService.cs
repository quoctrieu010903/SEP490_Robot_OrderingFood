using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ISettingsService
    {
        Task<BaseResponseModel<PaymentPolicy>> GetPaymentPolicyAsync();
        Task<BaseResponseModel<PaymentPolicy>> UpdatePaymentPolicyAsync(PaymentPolicy policy);
    }
}


