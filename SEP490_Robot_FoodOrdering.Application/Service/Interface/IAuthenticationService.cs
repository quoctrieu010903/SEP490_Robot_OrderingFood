using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.User;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Authentication;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.User;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
        public interface IAuthenticationService
        {
            Task<BaseResponseModel<AuthenticationResponse>> SignInAsync(SignInRequest request);
            Task<BaseResponseModel<UserProfileResponse>> GetProfileAsync();
            Task<BaseResponseModel<bool>> UpdateProfileAsync(UpdateProfileRequest request);

           
          
        }

}

