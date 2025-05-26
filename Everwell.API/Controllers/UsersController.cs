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
    }
}
