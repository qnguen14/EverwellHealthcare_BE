using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Exceptions;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.Auth;
using Everwell.DAL.Data.Responses.Auth;
using Everwell.DAL.Data.Requests.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

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
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.Login(request);
            
            if (response == null)
            {
                return NotFound(new ApiResponse<LoginResponse>
                {
                    Message = "Tài khoản không tồn tại.",
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound
                });
            }
            
            if (response.IsUnauthorized)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Message = "Mật khẩu không đúng.",
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            } 
            
            var apiResponse = new ApiResponse<LoginResponse>
            {
                Message = "Đăng nhập thành công.",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = response
            };
            
            return Ok(apiResponse);
            
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
[HttpPost(ApiEndpointConstants.Auth.RegisterEndpoint)]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    if (request == null)
    {
        return BadRequest("Invalid registration request.");
    }

    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
        var response = await _authService.Register(request);
        
        if (response.Success)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, 
            new { Success = false, Message = "An error occurred during registration." });
    }
}

[HttpPost(ApiEndpointConstants.Auth.LogoutEndpoint)]
[Authorize]
public async Task<IActionResult> Logout()
{
    try
    {
        // Get the token from the Authorization header
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return BadRequest("Authorization header is missing.");
        }

        var response = await _authService.Logout(authHeader);
        
        if (response.Success)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, 
            new { Success = false, Message = "An error occurred during logout." });
    }
}

    }
}
