using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using System.Security.Claims;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController(IAuthenticationService authenticationService, ILocalizationService localizationService) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest();

            var authResponse = await authenticationService.Register(request);
            return authResponse.Result ? Ok(authResponse) : BadRequest(authResponse);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(UserLoginRequest requestUser)
        {
            if (!ModelState.IsValid) return BadRequest();

            var authResponse = await authenticationService.Login(requestUser);
            return authResponse.Result ? Ok(authResponse) : BadRequest(authResponse);
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest();

            var userId = User.FindFirstValue("id");
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var response = await authenticationService.ChangePassword(parsedUserId, request);
            return response.Result ? Ok(response) : BadRequest(response);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = [localizationService.GetLocalizedString("AuthInvalidParameters")]
                });
            }

            var authResponse = await authenticationService.RefreshToken(tokenRequest);
            return authResponse.Result ? Ok(authResponse) : BadRequest(authResponse);
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue("id");
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var response = await authenticationService.Logout(parsedUserId);
            return response.Result ? Ok(response) : BadRequest(response);
        }
    }
}