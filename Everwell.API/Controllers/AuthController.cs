using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Requests.Auth;
using Everwell.DAL.Data.Requests.User;
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

 [HttpPost("send-reset-code")]
public async Task<IActionResult> SendResetCode([FromBody] ForgotPasswordRequest request)
{
    if (request == null || string.IsNullOrEmpty(request.Email))
    {
        return BadRequest("Email is required.");
    }

    try
    {
        var result = await _authService.SendPasswordResetCodeAsync(request.Email);
        
        return Ok(new { message = "If an account with that email exists, a verification code has been sent." });
    }
    catch (Exception ex)
    {
        return StatusCode(500, "An error occurred while processing your request.");
    }
}

[HttpPost("verify-code-and-reset")]
public async Task<IActionResult> VerifyCodeAndReset([FromBody] VerifyCodeAndResetRequest request)
{
    if (request == null || string.IsNullOrEmpty(request.Code) || 
        string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
    {
        return BadRequest("Code, email, and new password are required.");
    }

    try
    {
        var result = await _authService.VerifyResetCodeAndResetPasswordAsync(
            request.Code, request.Email, request.NewPassword);
        
        if (!result)
        {
            return BadRequest("Invalid or expired verification code.");
        }

        return Ok(new { message = "Password has been reset successfully." });
    }
    catch (Exception ex)
    {
        return StatusCode(500, "An error occurred while resetting your password.");
    }
}

    }
}
