using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.SystemSettings;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class SettingsService : ISettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SettingsService> _logger;

        public SettingsService(IUnitOfWork unitOfWork, ILogger<SettingsService> logger , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;

        }

        public async Task<BaseResponseModel<IEnumerable<SystemSettingResponse>>> GetAllAsync()
        {
            var settings = await _unitOfWork.Repository<SystemSettings, Guid>().GetAllAsync();
            var responseSettings = _mapper.Map<IEnumerable<SystemSettingResponse>>(settings);
            return new BaseResponseModel<IEnumerable<SystemSettingResponse>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                responseSettings
            );
        }


        public async Task<BaseResponseModel<SystemSettingResponse>> GetByIdAsync(Guid id)
        {
            var existingSetting = await _unitOfWork.Repository<SystemSettings, Guid>().GetByIdAsync(id);
            if (existingSetting != null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, "NOT_FOUND", "Setting not found");
            }
            var response = _mapper.Map<SystemSettingResponse>(existingSetting);
            return new BaseResponseModel<SystemSettingResponse>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                response
            );
        }



        public async Task<BaseResponseModel<SystemSettingResponse>> GetByKeyAsync(string key)
        {
            var settings = await _unitOfWork.Repository<SystemSettings, Guid>()
                .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => s.Key == key));
            if (settings == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, "NOT_FOUND", "Setting not found");
            }
            var response = _mapper.Map<SystemSettingResponse>(settings);
            return new BaseResponseModel<SystemSettingResponse>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                response
            );

        }

        public async Task<BaseResponseModel<PaymentPolicy>> GetPaymentPolicyAsync()
        {
            var settings = (await _unitOfWork.Repository<SystemSettings, Guid>().GetAllAsync())
                .FirstOrDefault(s => s.Key == "PaymentPolicy");
            if (settings == null)
            {
                // Lazy create default if missing
                settings = new SystemSettings 
                { 
                    Id = Guid.NewGuid(), 
                    Key = "PaymentPolicy",
                    Value = PaymentPolicy.Postpay.ToString(),
                    Type = SettingType.String
                };
                await _unitOfWork.Repository<SystemSettings, Guid>().AddAsync(settings);
                await _unitOfWork.SaveChangesAsync();
            }
            var policy = Enum.Parse<PaymentPolicy>(settings.Value);
            return new BaseResponseModel<PaymentPolicy>(StatusCodes.Status200OK, "SUCCESS", policy);
        }

        public async Task<BaseResponseModel<bool>> UpdateByIdAsync(Guid id, string value)
        {
            var setting = await _unitOfWork.Repository<SystemSettings, Guid>().GetByIdAsync(id);

            if (setting == null)
                throw new ErrorException(StatusCodes.Status404NotFound, "NOT_FOUND", "Setting not found");

            setting.Value = value;
            setting.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.Repository<SystemSettings, Guid>().Update(setting);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponseModel<bool>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                true,
                null,
                "Cập nhật cài đặt hệ thống thành công"
            );
        }


        public async Task<BaseResponseModel<PaymentPolicy>> UpdatePaymentPolicyAsync(PaymentPolicy policy)
        {
            var settings = (await _unitOfWork.Repository<SystemSettings, Guid>().GetAllAsync())
                .FirstOrDefault(s => s.Key == "PaymentPolicy");
            if (settings == null)
            {
                settings = new SystemSettings 
                { 
                    Id = Guid.NewGuid(), 
                    Key = "PaymentPolicy",
                    Value = policy.ToString(),
                    Type = SettingType.String
                };
                await _unitOfWork.Repository<SystemSettings, Guid>().AddAsync(settings);
            }
            else
            {
                settings.Value = policy.ToString();
                await _unitOfWork.Repository<SystemSettings, Guid>().UpdateAsync(settings);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Payment policy updated to {Policy}", policy);
            return new BaseResponseModel<PaymentPolicy>(StatusCodes.Status200OK, "UPDATED", policy);
        }

        public async Task<BaseResponseModel<bool>> UpdateValueAsync(string key, string value)
        {
            var existingSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => s.Key == key));

            if (existingSetting == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, "NOT_FOUND", "Setting not found");
            }

            existingSetting.Value = value;
            existingSetting.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.Repository<SystemSettings, Guid>().Update(existingSetting);

            // 4. Lưu thay đổi vào DB
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel<bool>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                true,
                null,
                "Cập nhật cài đặt hệ thống thành công"
            );
        }
    }
}


