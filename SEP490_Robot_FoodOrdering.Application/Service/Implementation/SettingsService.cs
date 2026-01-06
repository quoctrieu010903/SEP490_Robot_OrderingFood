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
            // Tính toán thời điểm có hiệu lực: 0h00 ngày hôm sau (theo timezone VN)
            var vnTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            var nowInVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz);
            var tomorrowMidnightVn = nowInVn.Date.AddDays(1); // 0h00 ngày hôm sau
            var effectiveDateUtc = TimeZoneInfo.ConvertTimeToUtc(tomorrowMidnightVn, vnTz);

            // Lưu PaymentPolicyPending
            var pendingSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => 
                    s.Key == SystemSettingKeys.PaymentPolicyPending && !s.DeletedTime.HasValue));
            
            if (pendingSetting == null)
            {
                pendingSetting = new SystemSettings 
                { 
                    Id = Guid.NewGuid(), 
                    Key = SystemSettingKeys.PaymentPolicyPending,
                    Value = policy.ToString(),
                    Type = SettingType.String,
                    DisplayName = "Chính sách thanh toán đang chờ áp dụng",
                    Description = "Chính sách thanh toán sẽ được áp dụng vào ngày hôm sau",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                };
                await _unitOfWork.Repository<SystemSettings, Guid>().AddAsync(pendingSetting);
            }
            else
            {
                pendingSetting.Value = policy.ToString();
                pendingSetting.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<SystemSettings, Guid>().Update(pendingSetting);
            }

            // Lưu PaymentPolicyEffectiveDate
            var effectiveDateSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => 
                    s.Key == SystemSettingKeys.PaymentPolicyEffectiveDate && !s.DeletedTime.HasValue));
            
            if (effectiveDateSetting == null)
            {
                effectiveDateSetting = new SystemSettings 
                { 
                    Id = Guid.NewGuid(), 
                    Key = SystemSettingKeys.PaymentPolicyEffectiveDate,
                    Value = effectiveDateUtc.ToString("O"), // ISO 8601 format
                    Type = SettingType.DateTime,
                    DisplayName = "Ngày giờ áp dụng chính sách thanh toán",
                    Description = "Thời điểm mà PaymentPolicyPending sẽ được áp dụng",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                };
                await _unitOfWork.Repository<SystemSettings, Guid>().AddAsync(effectiveDateSetting);
            }
            else
            {
                effectiveDateSetting.Value = effectiveDateUtc.ToString("O");
                effectiveDateSetting.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<SystemSettings, Guid>().Update(effectiveDateSetting);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation(
                "Payment policy scheduled to update to {Policy} at {EffectiveDate} (VN time: {EffectiveDateVn})", 
                policy, effectiveDateUtc, tomorrowMidnightVn);
            
            return new BaseResponseModel<PaymentPolicy>(
                StatusCodes.Status200OK, 
                "SCHEDULED", 
                policy,
                null,
                $"Chính sách thanh toán sẽ được áp dụng vào {tomorrowMidnightVn:dd/MM/yyyy HH:mm}");
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


