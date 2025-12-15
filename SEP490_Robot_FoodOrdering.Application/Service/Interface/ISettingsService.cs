using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.SystemSettings;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ISettingsService
    {
        Task<BaseResponseModel<PaymentPolicy>> GetPaymentPolicyAsync();
        Task<BaseResponseModel<PaymentPolicy>> UpdatePaymentPolicyAsync(PaymentPolicy policy);

        Task<BaseResponseModel<IEnumerable<SystemSettingResponse>>> GetAllAsync();
        Task<BaseResponseModel<SystemSettingResponse>> GetByKeyAsync(string key);
        Task<BaseResponseModel<SystemSettingResponse>> GetByIdAsync(Guid id);
        Task<BaseResponseModel<bool>> UpdateValueAsync(string key, string value);
          Task<BaseResponseModel<bool>> UpdateByIdAsync(Guid id, string value);
        //Task<BaseResponseModel<SystemSettingResponse>> CreateAsync(CreateSystemSettingRequest request);


    }
}


