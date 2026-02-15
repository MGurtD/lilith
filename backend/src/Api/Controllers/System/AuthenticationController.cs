using Microsoft.AspNetCore.Mvc;
using Application.Contracts;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController(IAuthenticationService authenticationService, ILocalizationService localizationService) : ControllerBase
    {
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Use the authentication service register
            var authResponse = await authenticationService.Register(request);
            return authResponse.Result ? Ok(authResponse) : BadRequest(authResponse);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(UserLoginRequest requestUser)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Use the authentication service register
            var authResponse = await authenticationService.Login(requestUser);
            return authResponse.Result ? Ok(authResponse) : BadRequest(authResponse);
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(UserLoginRequest requestUser)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Use the authentication service register
            var changed = await authenticationService.ChangePassword(requestUser);
            return changed ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
        }

        // TODO > Implementar refresh tokens quan hagi separat les responsabilitats de autenticació i autorització
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

    }
}