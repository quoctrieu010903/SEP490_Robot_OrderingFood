using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Cloudinary;
using SEP490_Robot_FoodOrdering.Application.Abstractions.JWT;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.User;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Authentication;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.User;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        public AuthenticationService(IUnitOfWork unitOfWork, IJwtService jwtService , IUtilsService utils , IHttpContextAccessor httpContextAccessor, IMapper mapper , ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _utils = utils;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PaginatedList<UserProfileResponse>> GetAllUser(PagingRequestModel paging)
        {
            var response = await _unitOfWork.Repository<User, Guid>().GetAllWithIncludeAsync(true , u=>u.Role);
            var mappedResponse = _mapper.Map<List<UserProfileResponse>>(response);
            return PaginatedList<UserProfileResponse>.Create(mappedResponse, paging.PageNumber, paging.PageSize);
        }

        public async Task<BaseResponseModel<UserProfileResponse>> GetProfileAsync()
        {
            var userid = Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst("Id")?.Value);
            var user = await _unitOfWork.Repository<User, Guid>()
                .GetByIdWithIncludeAsync(
                    (u => u.Id == userid),
                    true,
                      u => u.Role
                );
            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, ErrorMessages.NOT_FOUND);
            }
           var response = _mapper.Map<UserProfileResponse>(user);
            return new BaseResponseModel<UserProfileResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, response);
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

        public async Task<BaseResponseModel<bool>> UpdateProfileAsync(UpdateProfileRequest request)
        {
            var userid = Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst("Id")?.Value);
            var user = await _unitOfWork.Repository<User, Guid>().GetByIdAsync(userid); // Added 'await' here
            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, ErrorMessages.NOT_FOUND);
            }
            var updatedUser = _mapper.Map<User>(request);
            if (request.Avatar is not null && request.Avatar.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(
                    request.Avatar,
                    "User_Avatar", // tên folder trên Cloudinary
                    null        // vì đang tạo mới nên không có ảnh cũ để xóa
                );
                updatedUser.Avartar = imageUrl;
            }
            await _unitOfWork.Repository<User, Guid>().UpdateAsync(updatedUser);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel<bool>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, true);
        }

    }
}
