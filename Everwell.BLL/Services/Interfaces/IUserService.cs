using Everwell.DAL.Data.Requests.User;
using Everwell.DAL.Data.Responses.User;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<CreateUserResponse>> GetUsers();
        Task<GetUserResponse> GetUserById(Guid id);
        Task<CreateUserResponse> CreateUser(CreateUserRequest request);
        Task<UpdateUserResponse> UpdateUser(Guid id, UpdateUserRequest request);
        Task<bool> DeleteUser(Guid id);

    }
}
