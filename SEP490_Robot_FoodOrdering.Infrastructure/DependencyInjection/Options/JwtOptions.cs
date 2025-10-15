

namespace SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Options
{
    public class JwtOptions
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDays { get; set; }
    }
}
