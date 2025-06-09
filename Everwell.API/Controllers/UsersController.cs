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
            var users = await _userService.GetUsers();
            return Ok(users);
        }

        [HttpPost(ApiEndpointConstants.User.CreateUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateUserResponse>> CreateUser(CreateUserRequest request)
        {
            var users = await _userService.CreateUser(request);
            return Ok(users);
        }

        [HttpGet(ApiEndpointConstants.User.GetUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<GetUserResponse>> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                return Ok(user);
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
                var user = await _userService.UpdateUser(id, request);
                return Ok(user);
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
        // [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUser(id);
                if (result)
                {
                    return NoContent();
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
                return Ok(user);
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
        [Authorize]
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
                return Ok(user);
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
        [Authorize]
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
                return Ok(user);
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
