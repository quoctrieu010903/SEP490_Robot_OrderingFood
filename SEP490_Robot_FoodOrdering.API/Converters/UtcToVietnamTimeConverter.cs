using System.Text.Json.Serialization;
using System.Text.Json;

namespace SEP490_Robot_FoodOrdering.API.Converters
{
    public class VietnamDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        // 📥 INPUT: Client gửi giờ VN → Convert sang UTC lưu DB
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeString = reader.GetString();

            if (DateTime.TryParse(dateTimeString, out DateTime parsedDateTime))
            {
                // Ví dụ: Client gửi "30/07/2025 12:55:00" (VN) 
                // → Convert thành "30/07/2025 05:55:00" (UTC) để lưu DB
                var vietnamDateTime = DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Unspecified);
                var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(vietnamDateTime, VietnamTimeZone);
                return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            throw new JsonException($"Unable to parse DateTime: {dateTimeString}");
        }

        // 📤 OUTPUT: DB trả UTC → Convert sang giờ VN cho client  
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Ví dụ: DB có "30/07/2025 05:55:00" (UTC)
            // → Convert thành "30/07/2025 12:55:00" (VN) trả về client
            DateTime utcDateTime = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();

            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
            writer.WriteStringValue(vietnamTime.ToString("dd/MM/yyyy HH:mm:ss"));

            // 🎯 KẾT QUẢ: Client nhận "30/07/2025 12:55:00" (giờ VN chính xác!)
        }
    }

}


