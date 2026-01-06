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
            // LƯU Ý: Không filter DeletedTime ở đây để tránh vi phạm unique index (Key unique)
            var pendingSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s =>
                    s.Key == SystemSettingKeys.PaymentPolicyPending));

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
                // Nếu record đã bị soft delete trước đó, reset DeletedTime để tái sử dụng
                pendingSetting.DeletedTime = null;
                pendingSetting.Value = policy.ToString();
                pendingSetting.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<SystemSettings, Guid>().Update(pendingSetting);
            }

            // Lưu PaymentPolicyEffectiveDate
            // Tương tự, không filter DeletedTime để tránh duplicate key
            var effectiveDateSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s =>
                    s.Key == SystemSettingKeys.PaymentPolicyEffectiveDate));

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
                // Nếu record đã bị soft delete trước đó, reset DeletedTime để tái sử dụng
                effectiveDateSetting.DeletedTime = null;
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

        public async Task<BaseResponseModel<bool>> ApplyPendingPaymentPolicyAsync(bool force = false)
        {
            try
            {
                var pendingSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => 
                        s.Key == SystemSettingKeys.PaymentPolicyPending && !s.DeletedTime.HasValue));

                if (pendingSetting == null)
                {
                    return new BaseResponseModel<bool>(
                        StatusCodes.Status404NotFound,
                        "NOT_FOUND",
                        false,
                        null,
                        "Không có PaymentPolicyPending để áp dụng"
                    );
                }

                var effectiveDateSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => 
                        s.Key == SystemSettingKeys.PaymentPolicyEffectiveDate && !s.DeletedTime.HasValue));

                if (effectiveDateSetting == null)
                {
                    return new BaseResponseModel<bool>(
                        StatusCodes.Status404NotFound,
                        "NOT_FOUND",
                        false,
                        null,
                        "PaymentPolicyPending tồn tại nhưng thiếu PaymentPolicyEffectiveDate"
                    );
                }

                // Parse effective date
                if (!DateTime.TryParse(effectiveDateSetting.Value, out var effectiveDateUtc))
                {
                    return new BaseResponseModel<bool>(
                        StatusCodes.Status400BadRequest,
                        "INVALID_FORMAT",
                        false,
                        null,
                        $"Định dạng PaymentPolicyEffectiveDate không hợp lệ: {effectiveDateSetting.Value}"
                    );
                }

                // Kiểm tra xem đã đến thời điểm áp dụng chưa (trừ khi force = true)
                var nowUtc = DateTime.UtcNow;
                if (!force && nowUtc < effectiveDateUtc)
                {
                    var vnTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                    var effectiveDateVn = TimeZoneInfo.ConvertTimeFromUtc(effectiveDateUtc, vnTz);
                    var nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTz);
                    return new BaseResponseModel<bool>(
                        StatusCodes.Status400BadRequest,
                        "NOT_YET_EFFECTIVE",
                        false,
                        null,
                        $"Chưa đến thời điểm áp dụng. Sẽ áp dụng vào {effectiveDateVn:dd/MM/yyyy HH:mm} (VN time). Hiện tại: {nowVn:dd/MM/yyyy HH:mm}"
                    );
                }

                // Áp dụng PaymentPolicyPending vào PaymentPolicy
                var currentPolicySetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => 
                        s.Key == SystemSettingKeys.PaymentPolicy && !s.DeletedTime.HasValue));

                var oldPolicy = currentPolicySetting?.Value;
                var newPolicy = pendingSetting.Value;

                if (currentPolicySetting == null)
                {
                    // Tạo mới nếu chưa có
                    currentPolicySetting = new SystemSettings
                    {
                        Id = Guid.NewGuid(),
                        Key = SystemSettingKeys.PaymentPolicy,
                        Value = pendingSetting.Value,
                        Type = SettingType.String,
                        DisplayName = "Chính sách thanh toán",
                        Description = "Prepay = thanh toán trước, Postpay = thanh toán sau",
                        CreatedTime = DateTime.UtcNow,
                        LastUpdatedTime = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<SystemSettings, Guid>().AddAsync(currentPolicySetting);
                }
                else
                {
                    currentPolicySetting.Value = pendingSetting.Value;
                    currentPolicySetting.LastUpdatedTime = DateTime.UtcNow;
                    _unitOfWork.Repository<SystemSettings, Guid>().Update(currentPolicySetting);
                }

                // Xóa PaymentPolicyPending và PaymentPolicyEffectiveDate sau khi đã áp dụng
                pendingSetting.DeletedTime = DateTime.UtcNow;
                effectiveDateSetting.DeletedTime = DateTime.UtcNow;
                _unitOfWork.Repository<SystemSettings, Guid>().Update(pendingSetting);
                _unitOfWork.Repository<SystemSettings, Guid>().Update(effectiveDateSetting);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Payment policy manually applied from {OldPolicy} to {NewPolicy} (force={Force})",
                    oldPolicy ?? "null", newPolicy, force);

                return new BaseResponseModel<bool>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    true,
                    null,
                    $"Đã áp dụng PaymentPolicy từ {oldPolicy ?? "null"} sang {newPolicy} thành công"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying pending payment policy");
                return new BaseResponseModel<bool>(
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR",
                    false,
                    null,
                    $"Lỗi khi áp dụng PaymentPolicyPending: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Set PaymentPolicyEffectiveDate to past (for testing only)
        /// This allows testing the apply logic immediately
        /// </summary>
        public async Task<BaseResponseModel<bool>> SetPaymentPolicyEffectiveDateToPastAsync()
        {
            try
            {
                var effectiveDateSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => 
                        s.Key == SystemSettingKeys.PaymentPolicyEffectiveDate && !s.DeletedTime.HasValue));

                if (effectiveDateSetting == null)
                {
                    return new BaseResponseModel<bool>(
                        StatusCodes.Status404NotFound,
                        "NOT_FOUND",
                        false,
                        null,
                        "Không tìm thấy PaymentPolicyEffectiveDate"
                    );
                }

                // Set effective date to 1 hour ago
                var pastDateUtc = DateTime.UtcNow.AddHours(-1);
                effectiveDateSetting.Value = pastDateUtc.ToString("O");
                effectiveDateSetting.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<SystemSettings, Guid>().Update(effectiveDateSetting);
                await _unitOfWork.SaveChangesAsync();

                var vnTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                var pastDateVn = TimeZoneInfo.ConvertTimeFromUtc(pastDateUtc, vnTz);

                _logger.LogWarning(
                    "PaymentPolicyEffectiveDate set to past for testing: {PastDateVn}",
                    pastDateVn);

                return new BaseResponseModel<bool>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    true,
                    null,
                    $"Đã set PaymentPolicyEffectiveDate về quá khứ: {pastDateVn:dd/MM/yyyy HH:mm} (VN time). Có thể gọi ApplyPendingPaymentPolicy ngay bây giờ."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting PaymentPolicyEffectiveDate to past");
                return new BaseResponseModel<bool>(
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR",
                    false,
                    null,
                    $"Lỗi khi set PaymentPolicyEffectiveDate: {ex.Message}"
                );
            }
        }
    }
}


