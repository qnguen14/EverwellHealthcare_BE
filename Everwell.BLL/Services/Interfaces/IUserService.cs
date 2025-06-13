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
        Task<IEnumerable<CreateUserResponse>> GetUsersByRole(string role);
        Task<GetUserResponse> GetUserById(Guid id);
        Task<CreateUserResponse> CreateUser(CreateUserRequest request);
        Task<UpdateUserResponse> UpdateUser(Guid id, UpdateUserRequest request);
        Task<bool> DeleteUser(Guid id);
        
        
        // Add these new methods for password reset functionality
        Task<GetUserResponse> GetUserByEmailAsync(string email);
        Task<bool> UpdatePasswordAsync(Guid userId, string newPassword);
        
        // New methods for admin role management and profile updates
        Task<UpdateUserResponse> SetUserRole(Guid userId, SetUserRoleRequest request);
        Task<UpdateUserResponse> UpdateProfile(Guid userId, UpdateProfileRequest request);
        Task<UpdateUserResponse> UpdateAvatar(Guid userId, UpdateAvatarRequest request);
        
        // New profile-specific methods
        Task<UserProfileResponse> GetUserProfile(Guid userId);
        Task<UserProfileResponse> GetCurrentUserProfile(Guid currentUserId);
        
    }
}
