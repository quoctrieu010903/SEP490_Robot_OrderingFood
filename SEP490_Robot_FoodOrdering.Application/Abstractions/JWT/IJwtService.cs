
using System.Security.Claims;

using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.JWT
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        (string refreshToken, DateTime refreshTokenExpired) GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    }
}
