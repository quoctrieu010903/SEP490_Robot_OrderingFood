using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class SettingsService : ISettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SettingsService> _logger;

        public SettingsService(IUnitOfWork unitOfWork, ILogger<SettingsService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResponseModel<PaymentPolicy>> GetPaymentPolicyAsync()
        {
            var settings = (await _unitOfWork.Repository<SystemSettings, Guid>().GetAllAsync()).FirstOrDefault();
            if (settings == null)
            {
                // Lazy create default if missing
                settings = new SystemSettings { Id = Guid.NewGuid(), PaymentPolicy = PaymentPolicy.Postpay };
                await _unitOfWork.Repository<SystemSettings, Guid>().AddAsync(settings);
                await _unitOfWork.SaveChangesAsync();
            }
            return new BaseResponseModel<PaymentPolicy>(StatusCodes.Status200OK, "SUCCESS", settings.PaymentPolicy);
        }

        public async Task<BaseResponseModel<PaymentPolicy>> UpdatePaymentPolicyAsync(PaymentPolicy policy)
        {
            var settings = (await _unitOfWork.Repository<SystemSettings, Guid>().GetAllAsync()).FirstOrDefault();
            if (settings == null)
            {
                settings = new SystemSettings { Id = Guid.NewGuid(), PaymentPolicy = policy };
                await _unitOfWork.Repository<SystemSettings, Guid>().AddAsync(settings);
            }
            else
            {
                settings.PaymentPolicy = policy;
                await _unitOfWork.Repository<SystemSettings, Guid>().UpdateAsync(settings);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Payment policy updated to {Policy}", policy);
            return new BaseResponseModel<PaymentPolicy>(StatusCodes.Status200OK, "UPDATED", policy);
        }
    }
}


