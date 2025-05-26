using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Requests.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost(ApiEndpointConstants.Auth.LoginEndpoint)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid login request.");
            }
            try
            {
                var response = await _authService.Login(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
