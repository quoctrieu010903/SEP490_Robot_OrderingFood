using System;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class UpdateBusinessSettingsRequest
    {
        public string? OpeningHours { get; set; } // legacy, sẽ được generate từ OpeningTime/ClosingTime
        public string? OpeningTime { get; set; }  // "HH:mm" 24h, ví dụ "10:00"
        public string? ClosingTime { get; set; }  // "HH:mm" 24h, ví dụ "22:00"
        public string? TaxRate { get; set; }
        public int? MaxTableCapacity { get; set; }
        public int? TableAccessTimeoutWithoutOrderMinutes { get; set; }
        public int? OrderCleanupAfterDays { get; set; }
        public string? RestaurantName { get; set; }
    }
}

