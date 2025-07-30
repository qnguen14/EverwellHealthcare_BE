// ============================================================================
// AUTHENTICATION CONTROLLER
// ============================================================================
// This controller handles all authentication-related operations for the Everwell system
// It manages user login, registration, password changes, and token management
// 
// FLOW EXPLANATION:
// 1. User submits login credentials (email/password)
// 2. AuthService validates credentials against database
// 3. If valid, JWT token is generated with user claims (ID, role, etc.)
// 4. Token is returned to client for subsequent API calls
// 5. Client includes token in Authorization header for protected endpoints

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

        /// <summary>
        /// USER LOGIN ENDPOINT
        /// ==================
        /// Authenticates user credentials and returns JWT token for API access
        /// 
        /// AUTHENTICATION FLOW:
        /// 1. Receive login request with email and password
        /// 2. AuthService validates credentials against database
        /// 3. If user exists and password matches (BCrypt verification):
        ///    - Generate JWT token with user claims (ID, role, email)
        ///    - Return token with user profile information
        /// 4. If credentials invalid, return appropriate error
        /// 
        /// SECURITY FEATURES:
        /// - Password hashing with BCrypt
        /// - JWT token with expiration
        /// - Role-based claims for authorization
        /// - Account status validation (active/inactive)
        /// </summary>
        [HttpPost(ApiEndpointConstants.Auth.LoginEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Call business logic layer to handle authentication
            var response = await _authService.Login(request);
            
            // Handle case where user account doesn't exist
            if (response == null)
            {
                return NotFound(new ApiResponse<LoginResponse>
                {
                    Message = "Tài khoản không tồn tại.", // "Account does not exist"
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound
                });
            }
            
            // Handle case where password is incorrect
            if (response.IsUnauthorized)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Message = "Mật khẩu không đúng.", // "Password is incorrect"
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            } 
            
            // Successful login - return JWT token and user information
            var apiResponse = new ApiResponse<LoginResponse>
            {
                Message = "Đăng nhập thành công.", // "Login successful"
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = response // Contains JWT token, user profile, role information
            };
            
            return Ok(apiResponse);
        }

        /// <summary>
        /// PASSWORD RESET - SEND VERIFICATION CODE
        /// =======================================
        /// Initiates password reset process by sending verification code to user's email
        /// 
        /// RESET FLOW STEP 1:
        /// 1. User provides email address for password reset
        /// 2. System validates email exists in database
        /// 3. Generate random verification code (6-digit)
        /// 4. Store code temporarily with expiration time
        /// 5. Send verification code via email service
        /// 6. Return generic success message (security best practice)
        /// 
        /// SECURITY CONSIDERATIONS:
        /// - Generic response prevents email enumeration attacks
        /// - Verification codes expire after set time period
        /// - Rate limiting should be implemented to prevent abuse
        /// </summary>
        [HttpPost("send-reset-code")]
        public async Task<IActionResult> SendResetCode([FromBody] ForgotPasswordRequest request)
        {
            // Validate input parameters
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required.");
            }

            try
            {
                // AuthService handles:
                // 1. Email validation and user lookup
                // 2. Verification code generation
                // 3. Email sending via IEmailService
                // 4. Temporary storage of reset code
                var result = await _authService.SendPasswordResetCodeAsync(request.Email);
                
                // Generic response for security (prevents email enumeration)
                return Ok(new { message = "If an account with that email exists, a verification code has been sent." });
            }
            catch (Exception ex)
            {
                // Log error details internally but return generic message
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// PASSWORD RESET - VERIFY CODE AND RESET PASSWORD
        /// ==============================================
        /// Completes password reset process by verifying code and updating password
        /// 
        /// RESET FLOW STEP 2:
        /// 1. User provides verification code, email, and new password
        /// 2. System validates verification code against stored value
        /// 3. Check code hasn't expired (time-based validation)
        /// 4. Hash new password using BCrypt
        /// 5. Update user's password in database
        /// 6. Invalidate/remove verification code
        /// 7. Return success confirmation
        /// 
        /// SECURITY FEATURES:
        /// - Code expiration validation
        /// - Password hashing with BCrypt
        /// - One-time use verification codes
        /// - Input validation for all parameters
        /// </summary>
        [HttpPost("verify-code-and-reset")]
        public async Task<IActionResult> VerifyCodeAndReset([FromBody] VerifyCodeAndResetRequest request)
        {
            // Validate all required parameters are provided
            if (request == null || string.IsNullOrEmpty(request.Code) || 
                string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("Code, email, and new password are required.");
            }

            try
            {
                // AuthService handles:
                // 1. Verification code validation
                // 2. Code expiration check
                // 3. Password hashing and database update
                // 4. Code cleanup/invalidation
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
        /// <summary>
        /// USER REGISTRATION ENDPOINT
        /// ==========================
        /// Creates new user account in the system with role assignment
        /// 
        /// REGISTRATION FLOW:
        /// 1. Validate registration request data (email, password, personal info)
        /// 2. Check email uniqueness in database
        /// 3. Hash password using BCrypt for security
        /// 4. Assign default role (typically 'Customer')
        /// 5. Create user record in database
        /// 6. Send welcome email (optional)
        /// 7. Return registration confirmation
        /// 
        /// VALIDATION RULES:
        /// - Email format validation
        /// - Password strength requirements
        /// - Required field validation
        /// - Duplicate email prevention
        /// 
        /// ROLE ASSIGNMENT:
        /// - Default role: Customer
        /// - Admin can register users with specific roles
        /// - Role determines system permissions
        /// </summary>
        [HttpPost(ApiEndpointConstants.Auth.RegisterEndpoint)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Basic null check for request object
            if (request == null)
            {
                return BadRequest("Invalid registration request.");
            }

            // Validate model state (data annotations validation)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // AuthService handles:
                // 1. Email uniqueness validation
                // 2. Password hashing
                // 3. User entity creation
                // 4. Role assignment
                // 5. Database persistence
                var response = await _authService.Register(request);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    // Return validation errors (e.g., email already exists)
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Success = false, Message = "An error occurred during registration." });
            }
        }

        /// <summary>
        /// USER LOGOUT ENDPOINT
        /// ====================
        /// Invalidates user's JWT token to prevent further API access
        /// 
        /// LOGOUT FLOW:
        /// 1. Extract JWT token from Authorization header
        /// 2. Add token to blacklist database table
        /// 3. Token becomes invalid for future requests
        /// 4. TokenBlacklistMiddleware checks blacklist on each request
        /// 5. Return logout confirmation
        /// 
        /// SECURITY IMPLEMENTATION:
        /// - Token blacklisting prevents token reuse
        /// - Middleware validates tokens against blacklist
        /// - Expired tokens are automatically cleaned up
        /// - Requires valid authentication to logout
        /// 
        /// TOKEN LIFECYCLE:
        /// - Active: Token valid and not blacklisted
        /// - Blacklisted: Token added to blacklist on logout
        /// - Expired: Token past expiration time (automatic)
        /// </summary>
        [HttpPost(ApiEndpointConstants.Auth.LogoutEndpoint)]
        [Authorize] // Requires valid JWT token to logout
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Extract JWT token from Authorization header ("Bearer <token>")
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(authHeader))
                {
                    return BadRequest("Authorization header is missing.");
                }

                // AuthService handles:
                // 1. Token extraction from "Bearer <token>" format
                // 2. Adding token to BlacklistedToken table
                // 3. Database persistence
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
