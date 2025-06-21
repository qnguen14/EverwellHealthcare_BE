using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.User;
using Everwell.DAL.Data.Responses.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
        }

        [HttpGet(ApiEndpointConstants.User.GetAllUsersEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateUserResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CreateUserResponse>>> GetUsers()
        {
            var response = await _userService.GetUsers();
            
            var apiResponse = new ApiResponse<IEnumerable<CreateUserResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Users retrieved successfully",
                IsSuccess = true,
                Data = response
            };
            return Ok(apiResponse);
        }
        
        [HttpGet(ApiEndpointConstants.User.GetUsersByRoleEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateUserResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, Consultant, Staff, Manager, Customer")]
        public async Task<ActionResult<IEnumerable<CreateUserResponse>>> GetUsersByRole(string role)
        {
            var response = await _userService.GetUsersByRole(role);
            var apiResponse = new ApiResponse<IEnumerable<CreateUserResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Users retrieved successfully",
                IsSuccess = true,
                Data = response
            };
            return Ok(apiResponse);
        }

        [HttpPost(ApiEndpointConstants.User.CreateUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateUserResponse>> CreateUser(CreateUserRequest request)
        {
            var response = await _userService.CreateUser(request);
            if (response == null)
            {
                return NotFound(new { message = "Tài khoản với email này đã tồn tại." });
            }
            var apiResponse = new ApiResponse<CreateUserResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "User created successfully",
                IsSuccess = true,
                Data = response
            };
            return Ok(apiResponse);
        }

        [HttpGet(ApiEndpointConstants.User.GetUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, Manager, Consultant, Staff, Customer")]
        public async Task<ActionResult<GetUserResponse>> GetUserById(Guid id)
        {
            try
            {
                var response = await _userService.GetUserById(id);
                
                var apiResponse = new ApiResponse<GetUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User retrieved successfully",
                    IsSuccess = true,
                    Data = response
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
        
        //to do: fix update and delete to follow other controller patterns

        [HttpPut(ApiEndpointConstants.User.UpdateUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateUserResponse>> UpdateUser(Guid id, UpdateUserRequest request)
        {
            try
            {
                var response = await _userService.UpdateUser(id, request);
                
                if (response == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User updated successfully",
                    IsSuccess = true,
                    Data = response
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpDelete(ApiEndpointConstants.User.DeleteUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUser(id);
                if (result)
                {
                    var apiResponse = new ApiResponse<CreateUserResponse>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "User deleted successfully",
                        IsSuccess = true,
                        Data = null // No data to return on delete
                    };
                    
                    return Ok(apiResponse);
                }
                return BadRequest(new { message = "Failed to delete user" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut(ApiEndpointConstants.User.SetRoleEndpoint)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateUserResponse>> SetUserRole(Guid id, SetUserRoleRequest request)
        {
            try
            {
                var user = await _userService.SetUserRole(id, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User role updated successfully",
                    IsSuccess = true,
                    Data = user
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut(ApiEndpointConstants.User.UpdateProfileEndpoint)]
        [Authorize(Roles = "Admin, Consultant, Staff, Manager, Customer")]
        public async Task<ActionResult<UpdateUserResponse>> UpdateProfile(Guid id, UpdateProfileRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                
                // Users can only update their own profile, unless they're admin
                if (currentUserRole != "Admin" && (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId) || currentUserId != id))
                {
                    return Forbid("You can only update your own profile.");
                }

                var user = await _userService.UpdateProfile(id, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User profile updated successfully",
                    IsSuccess = true,
                    Data = user
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut(ApiEndpointConstants.User.UpdateAvatarEndpoint)]
        [Authorize(Roles = "Admin, Consultant, Staff, Manager, Customer")]
        public async Task<ActionResult<UpdateUserResponse>> UpdateAvatar(Guid id, UpdateAvatarRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                
                // Users can only update their own avatar, unless they're admin
                if (currentUserRole != "Admin" && (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId) || currentUserId != id))
                {
                    return Forbid("You can only update your own avatar.");
                }

                var user = await _userService.UpdateAvatar(id, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User avatar updated successfully",
                    IsSuccess = true,
                    Data = user
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // New Profile API Endpoints

        [HttpGet(ApiEndpointConstants.User.GetMyProfileEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, Consultant, Staff, Manager, Customer")]
        public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
        {
            try
            {
                // Get current user ID from JWT token
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized(new { message = "Invalid user token." });
                }

                var profile = await _userService.GetCurrentUserProfile(currentUserId);
                
                var apiResponse = new ApiResponse<UserProfileResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User profile retrieved successfully",
                    IsSuccess = true,
                    Data = profile
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut(ApiEndpointConstants.User.UpdateMyProfileEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<UpdateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<ActionResult<UpdateUserResponse>> UpdateMyProfile(UpdateProfileRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized(new { message = "Invalid user token." });
                }

                var user = await _userService.UpdateProfile(currentUserId, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User profile updated successfully",
                    IsSuccess = true,
                    Data = user
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut(ApiEndpointConstants.User.UpdateMyAvatarEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<UpdateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<ActionResult<UpdateUserResponse>> UpdateMyAvatar(UpdateAvatarRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized(new { message = "Invalid user token." });
                }

                var user = await _userService.UpdateAvatar(currentUserId, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User avatar updated successfully",
                    IsSuccess = true,
                    Data = user
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
