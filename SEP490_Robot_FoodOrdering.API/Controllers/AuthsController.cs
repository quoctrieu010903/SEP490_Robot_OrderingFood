using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.User;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;

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
    }
}
