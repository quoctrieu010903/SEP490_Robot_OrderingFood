
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SEP490_Robot_FoodOrdering.Application.Abstractions.JWT;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Options;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Jwt
{
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IUtilsService _utils;
        public JwtService(IOptions<JwtOptions> jwtOptions, IUtilsService utils)
        {
            _jwtOptions = jwtOptions.Value;
            _utils = utils;
        }

        public string GenerateAccessToken(User user)
        {

            var claims = new[]{
                 new Claim("Id", user.Id.ToString()),
                 new Claim("Name", user.FullName),
                 new Claim("Role", user.Role.Name.ToString()),
                 new Claim("Email", user?.Email ?? ""),
                 new Claim("Phone", user?.PhoneNumber ?? "")
         };
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokenOptions = new JwtSecurityToken(
                    issuer: _jwtOptions.Issuer,
                    audience: _jwtOptions.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
                    signingCredentials: signingCredentials
                );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return token;
        }

        public (string refreshToken, DateTime refreshTokenExpired) GenerateRefreshToken()
        {
            return (refreshToken: _utils.GenerateRandomString(40, CharacterSet.Mixed)
                , DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays));
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var Key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, // product make it true
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            JwtSecurityToken? jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken is null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }

}
