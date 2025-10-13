using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Authentication
{
    public class AuthenticationResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpired { get; set; }
        public AuthenticationResponse(string accessToken, string refreshToken, DateTime refreshTokenExpired)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            RefreshTokenExpired = refreshTokenExpired;
        }

    }
}
