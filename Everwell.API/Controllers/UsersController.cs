using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
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

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet(ApiEndpointConstants.User.GetAllUsersEndpoint)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CreateUserResponse>>> GetUsers()
        {
            var users = await _userService.GetUsers();
            return Ok(users);
        }

        [HttpPost(ApiEndpointConstants.User.CreateUserEndpoint)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateUserResponse>> CreateUser(CreateUserRequest request)
        {
            var users = await _userService.CreateUser(request);
            return Ok(users);
        }

        [HttpGet(ApiEndpointConstants.User.GetUserEndpoint)]
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

        [HttpPut(ApiEndpointConstants.User.UpdateUserEndpoint)]
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
    }
}
