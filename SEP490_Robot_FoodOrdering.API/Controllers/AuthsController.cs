using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.User;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.User;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        public readonly IAuthenticationService _authenticationService;
        public AuthsController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] Application.DTO.Request.SignInRequest request)
        {
            var response = await _authenticationService.SignInAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize]
        [HttpPatch("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var response = await _authenticationService.UpdateProfileAsync(request);
            return Ok();
        }
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var response = await _authenticationService.GetProfileAsync();
            return StatusCode(response.StatusCode, response);
        }
        [Authorize()]
        [HttpGet("users")]
        public async Task<ActionResult<PaginatedList<UserProfileResponse>>> GetAll(
      [FromQuery] PagingRequestModel paging)
        {
            var result = await _authenticationService.GetAllUser(paging);
            return Ok(result);
        }

    }
}
