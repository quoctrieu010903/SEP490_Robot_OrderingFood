using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.SystemSettings;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using System.Text.RegularExpressions;

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

            var validation = ValidateAndNormalizeSetting(setting.Key, value);
            if (!validation.IsValid)
            {
                return validation.ErrorResponse!;
            }

            var normalizedValue = validation.NormalizedValue ?? value;
            setting.Value = normalizedValue;
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

            // Validate & normalize theo từng key
            var validation = ValidateAndNormalizeSetting(key, value);
            if (!validation.IsValid)
            {
                return validation.ErrorResponse!;
            }

            var normalizedValue = validation.NormalizedValue ?? value;
            existingSetting.Value = normalizedValue;
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

        private (bool IsValid, string? NormalizedValue, BaseResponseModel<bool>? ErrorResponse) ValidateAndNormalizeSetting(string key, string value)
        {
            // Nếu value null hoặc rỗng
            if (string.IsNullOrWhiteSpace(value))
            {
                return (false, null, BuildValidationError("Giá trị không được để trống"));
            }

            // Các key cần kiểm soát kiểu dữ liệu
            switch (key)
            {
                case SystemSettingKeys.RestaurantName:
                    if (value.Length > 200)
                        return (false, null, BuildValidationError("RestaurantName tối đa 200 ký tự"));
                    return (true, value.Trim(), null);

                case SystemSettingKeys.OpeningTime:
                case SystemSettingKeys.ClosingTime:
                    if (!TryParseTime24h(value, out var time))
                        return (false, null, BuildValidationError($"{key} phải theo định dạng HH:mm 24h, ví dụ: 10:00"));
                    return (true, time!.Value.ToString(@"hh\:mm"), null);

                case SystemSettingKeys.OpeningHours:
                    var pattern = @"^\s*\d{1,2}:\d{2}\s*(AM|PM)\s*-\s*\d{1,2}:\d{2}\s*(AM|PM)\s*$";
                    if (!Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                        return (false, null, BuildValidationError("OpeningHours phải theo định dạng ví dụ: 10:00 AM - 10:00 PM"));
                    if (value.Length > 100)
                        return (false, null, BuildValidationError("OpeningHours tối đa 100 ký tự"));
                    return (true, value.Trim(), null);

                case SystemSettingKeys.TaxRate:
                    var raw = value.Trim().Replace("%", "").Replace(",", ".");
                    if (!decimal.TryParse(raw, out var taxValue))
                        return (false, null, BuildValidationError("TaxRate phải là số (ví dụ 8 hoặc 8%)"));
                    if (taxValue < 0 || taxValue > 100)
                        return (false, null, BuildValidationError("TaxRate phải trong khoảng 0 - 100 (%)"));
                    var normalizedTax = $"{taxValue:0.##}%";
                    return (true, normalizedTax, null);

                case SystemSettingKeys.MaxTableCapacity:
                case SystemSettingKeys.TableAccessTimeoutWithoutOrderMinutes:
                case SystemSettingKeys.OrderCleanupAfterDays:
                    if (!int.TryParse(value, out var intVal))
                        return (false, null, BuildValidationError($"{key} phải là số nguyên"));

                    if (key == SystemSettingKeys.MaxTableCapacity && (intVal <= 0 || intVal > 1000))
                        return (false, null, BuildValidationError("MaxTableCapacity phải > 0 và ≤ 1000"));

                    if (key == SystemSettingKeys.TableAccessTimeoutWithoutOrderMinutes && (intVal <= 0 || intVal > 240))
                        return (false, null, BuildValidationError("TableAccessTimeoutWithoutOrderMinutes phải > 0 và ≤ 240 phút"));

                    if (key == SystemSettingKeys.OrderCleanupAfterDays && (intVal <= 0 || intVal > 365))
                        return (false, null, BuildValidationError("OrderCleanupAfterDays phải > 0 và ≤ 365 ngày"));

                    return (true, intVal.ToString(), null);

                default:
                    // Các key khác giữ nguyên, không ép kiểu
                    return (true, value, null);
            }
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

        public async Task<BaseResponseModel<bool>> UpdateBusinessSettingsAsync(UpdateBusinessSettingsRequest request)
        {
            if (request == null ||
                request.OpeningHours == null &&
                request.OpeningTime == null &&
                request.ClosingTime == null &&
                request.TaxRate == null &&
                request.MaxTableCapacity == null &&
                request.TableAccessTimeoutWithoutOrderMinutes == null &&
                request.OrderCleanupAfterDays == null &&
                request.RestaurantName == null)
            {
                return new BaseResponseModel<bool>(
                    StatusCodes.Status400BadRequest,
                    ResponseCodeConstants.INVALID_REQUEST,
                    false,
                    null,
                    "Không có trường nào được cung cấp để cập nhật");
            }

            // Validation & normalization
            if (request.RestaurantName != null)
            {
                if (string.IsNullOrWhiteSpace(request.RestaurantName))
                {
                    return BuildValidationError("RestaurantName không được để trống");
                }
                if (request.RestaurantName.Length > 200)
                {
                    return BuildValidationError("RestaurantName tối đa 200 ký tự");
                }
            }

            // OpeningTime / ClosingTime (24h, HH:mm)
            TimeSpan? openingTime = null;
            TimeSpan? closingTime = null;
            if (request.OpeningTime != null)
            {
                if (string.IsNullOrWhiteSpace(request.OpeningTime))
                    return BuildValidationError("OpeningTime không được để trống");

                if (!TryParseTime24h(request.OpeningTime, out openingTime))
                    return BuildValidationError("OpeningTime phải theo định dạng HH:mm 24h, ví dụ: 10:00");
            }

            if (request.ClosingTime != null)
            {
                if (string.IsNullOrWhiteSpace(request.ClosingTime))
                    return BuildValidationError("ClosingTime không được để trống");

                if (!TryParseTime24h(request.ClosingTime, out closingTime))
                    return BuildValidationError("ClosingTime phải theo định dạng HH:mm 24h, ví dụ: 22:00");
            }

            // Nếu cả 2 cùng có, kiểm tra OpeningTime < ClosingTime
            if (openingTime.HasValue && closingTime.HasValue)
            {
                if (openingTime.Value >= closingTime.Value)
                    return BuildValidationError("OpeningTime phải nhỏ hơn ClosingTime");
            }

            // OpeningHours (legacy) – nếu được gửi trực tiếp vẫn cho phép, nhưng khuyến khích dùng OpeningTime/ClosingTime
            if (request.OpeningHours != null)
            {
                if (string.IsNullOrWhiteSpace(request.OpeningHours))
                {
                    return BuildValidationError("OpeningHours không được để trống");
                }
                var pattern = @"^\s*\d{1,2}:\d{2}\s*(AM|PM)\s*-\s*\d{1,2}:\d{2}\s*(AM|PM)\s*$";
                if (!Regex.IsMatch(request.OpeningHours, pattern, RegexOptions.IgnoreCase))
                {
                    return BuildValidationError("OpeningHours phải theo định dạng ví dụ: 10:00 AM - 10:00 PM");
                }
                if (request.OpeningHours.Length > 100)
                {
                    return BuildValidationError("OpeningHours tối đa 100 ký tự");
                }
            }

            string? normalizedTax = null;
            if (request.TaxRate != null)
            {
                if (string.IsNullOrWhiteSpace(request.TaxRate))
                {
                    return BuildValidationError("TaxRate không được để trống");
                }
                var raw = request.TaxRate.Trim().Replace("%", "").Replace(",", ".");
                if (!decimal.TryParse(raw, out var taxValue))
                {
                    return BuildValidationError("TaxRate phải là số (ví dụ 8 hoặc 8%)");
                }
                if (taxValue < 0 || taxValue > 100)
                {
                    return BuildValidationError("TaxRate phải trong khoảng 0 - 100 (%)");
                }
                normalizedTax = $"{taxValue:0.##}%";
            }

            if (request.MaxTableCapacity != null)
            {
                if (request.MaxTableCapacity <= 0 || request.MaxTableCapacity > 1000)
                {
                    return BuildValidationError("MaxTableCapacity phải > 0 và ≤ 1000");
                }
            }

            if (request.TableAccessTimeoutWithoutOrderMinutes != null)
            {
                if (request.TableAccessTimeoutWithoutOrderMinutes <= 0 || request.TableAccessTimeoutWithoutOrderMinutes > 240)
                {
                    return BuildValidationError("TableAccessTimeoutWithoutOrderMinutes phải > 0 và ≤ 240 phút");
                }
            }

            if (request.OrderCleanupAfterDays != null)
            {
                if (request.OrderCleanupAfterDays <= 0 || request.OrderCleanupAfterDays > 365)
                {
                    return BuildValidationError("OrderCleanupAfterDays phải > 0 và ≤ 365 ngày");
                }
            }

            // Apply updates
            if (request.RestaurantName != null)
            {
                await UpsertSettingAsync(SystemSettingKeys.RestaurantName, request.RestaurantName, SettingType.String);
            }

            // Ưu tiên cặp OpeningTime/ClosingTime
            if (openingTime.HasValue)
            {
                await UpsertSettingAsync(SystemSettingKeys.OpeningTime, openingTime.Value.ToString(@"hh\:mm"), SettingType.String);
            }

            if (closingTime.HasValue)
            {
                await UpsertSettingAsync(SystemSettingKeys.ClosingTime, closingTime.Value.ToString(@"hh\:mm"), SettingType.String);
            }

            // Nếu có cả 2 time, sync lại OpeningHours dạng 12h cho legacy UI
            if (openingTime.HasValue && closingTime.HasValue)
            {
                var openingHoursText = $"{To12HourFormat(openingTime.Value)} - {To12HourFormat(closingTime.Value)}";
                await UpsertSettingAsync(SystemSettingKeys.OpeningHours, openingHoursText, SettingType.String);
            }
            else if (request.OpeningHours != null)
            {
                // Nếu chỉ gửi OpeningHours (legacy) mà không có time mới, vẫn cho update
                await UpsertSettingAsync(SystemSettingKeys.OpeningHours, request.OpeningHours, SettingType.String);
            }

            if (request.TaxRate != null)
            {
                await UpsertSettingAsync(SystemSettingKeys.TaxRate, normalizedTax!, SettingType.String);
            }

            if (request.MaxTableCapacity != null)
            {
                await UpsertSettingAsync(SystemSettingKeys.MaxTableCapacity, request.MaxTableCapacity.Value.ToString(), SettingType.Int);
            }

            if (request.TableAccessTimeoutWithoutOrderMinutes != null)
            {
                await UpsertSettingAsync(SystemSettingKeys.TableAccessTimeoutWithoutOrderMinutes, request.TableAccessTimeoutWithoutOrderMinutes.Value.ToString(), SettingType.Int);
            }

            if (request.OrderCleanupAfterDays != null)
            {
                await UpsertSettingAsync(SystemSettingKeys.OrderCleanupAfterDays, request.OrderCleanupAfterDays.Value.ToString(), SettingType.Int);
            }

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponseModel<bool>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                true,
                null,
                "Cập nhật cài đặt thành công");
        }

        private BaseResponseModel<bool> BuildValidationError(string message)
        {
            return new BaseResponseModel<bool>(
                StatusCodes.Status400BadRequest,
                ResponseCodeConstants.INVALID_REQUEST,
                false,
                null,
                message);
        }

        private async Task UpsertSettingAsync(string key, string value, SettingType type)
        {
            var setting = await _unitOfWork.Repository<SystemSettings, Guid>()
                .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => s.Key == key));

            if (setting == null)
            {
                setting = new SystemSettings
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    Type = type,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                };
                await _unitOfWork.Repository<SystemSettings, Guid>().AddAsync(setting);
            }
            else
            {
                setting.DeletedTime = null; // revive if soft-deleted
                setting.Value = value;
                setting.Type = type;
                setting.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<SystemSettings, Guid>().Update(setting);
            }
        }

        private static bool TryParseTime24h(string input, out TimeSpan? time)
        {
            time = null;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (TimeSpan.TryParseExact(input.Trim(), @"hh\:mm", null, out var parsed))
            {
                time = parsed;
                return true;
            }

            return false;
        }

        private static string To12HourFormat(TimeSpan time)
        {
            // Chuyển TimeSpan (24h) sang chuỗi 12h, ví dụ "10:00 AM"
            var dateTime = DateTime.Today.Add(time);
            return dateTime.ToString("hh:mm tt");
        }
    }
}


