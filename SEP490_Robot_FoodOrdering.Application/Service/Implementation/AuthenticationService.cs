using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.Abstractions.JWT;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Authentication;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class AuthenticationService : IAuthenticationService

    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService; 
        private readonly IUtilsService _utils;
        public AuthenticationService(IUnitOfWork unitOfWork, IJwtService jwtService , IUtilsService utils)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _utils = utils;
        }

        public async Task<BaseResponseModel<AuthenticationResponse>> SignInAsync(SignInRequest request)
        {
            var user = await _unitOfWork.Repository<User, Guid>()
                .GetByIdWithIncludeAsync(
                    (u => u.UserName == request.Username),
                    true,
                      u => u.Role 
                );
            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, ErrorMessages.NOT_FOUND);
            }
            var isValid = _utils.VerifyPassword(request.Password, user.Password);
            if (!isValid)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, ErrorMessages.INVALID_PASSWORD);
            }
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshTokenData = _jwtService.GenerateRefreshToken();
            var response = new AuthenticationResponse(accessToken, refreshTokenData.refreshToken,
                         refreshTokenData.refreshTokenExpired);
            return new BaseResponseModel<AuthenticationResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, response);
        }
                                                                                                                                                                                                                                                            
    }
}
