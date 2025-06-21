using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Responses.User;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Everwell.DAL.Data.Requests.User;
using AutoMapper;
using Everwell.DAL.Data.Exceptions;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;

namespace Everwell.BLL.Services.Implements
{
    public class UserService : BaseService<UserService>, IUserService
    {
        public UserService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<UserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<IEnumerable<CreateUserResponse>> GetUsers()
        {
            try
            {
                var users = await _unitOfWork.GetRepository<User>()
                    .GetListAsync(
                        predicate: u => u.IsActive,
                        include: u => u.Include(x => x.Role),
                        orderBy: u => u.OrderBy(n => n.Name)
                    );

                if (users == null || !users.Any())
                {
                    throw new NotFoundException("No active users found.");
                }

                return _mapper.Map<IEnumerable<CreateUserResponse>>(users);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving users: " + ex.Message);
                throw;
            }
        }
        
        public async Task<IEnumerable<CreateUserResponse>> GetUsersByRole(string role) 
        {
            try
            {
                if (string.IsNullOrEmpty(role))
                {
                    throw new ArgumentException("Role cannot be null or empty.", nameof(role));
                }

                if (!Enum.TryParse<RoleName>(role, true, out var parsedRole))
                {
                    throw new ArgumentException($"Invalid role: {role}. Valid roles are: {string.Join(", ", Enum.GetNames(typeof(RoleName)))}");
                }

                var users = await _unitOfWork.GetRepository<User>()
                    .GetListAsync(
                        predicate: u => u.Role.Name == parsedRole && u.IsActive,
                        include: u => u.Include(x => x.Role),
                        orderBy: u => u.OrderBy(n => n.Name)
                    );

                if (users == null || !users.Any())
                {
                    throw new NotFoundException($"No active users found with role: {role}");
                }

                return _mapper.Map<IEnumerable<CreateUserResponse>>(users);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving users by role: " + ex.Message);
                throw;
            }
        }


        public async Task<CreateUserResponse> CreateUser(CreateUserRequest request)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    if (request == null)
                    {
                        throw new ArgumentNullException(nameof(request), "Request cannot be null.");
                    }

                    // Validate the user entity (e.g., check for existing email)
                    var existingUser = await _unitOfWork.GetRepository<User>()
                        .FirstOrDefaultAsync(
                                predicate: u => u.Email == request.Email && u.IsActive);
                    if (existingUser != null)
                    {
                        return null;
                    }

                    // Map the basic fields
                    var newUser = _mapper.Map<User>(request);
                    
                    // Handle the fields that need special processing
                    
                    // 1. Generate a new ID
                    newUser.Id = Guid.NewGuid();
                    
                    // 2. Hash the password
                    newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    
                    // 3. Parse and set the role
                    if (Enum.TryParse<RoleName>(request.Role, true, out var roleName))
                    {
                        var role = await _unitOfWork.GetRepository<Role>()
                            .FirstOrDefaultAsync(predicate: r => r.Name == roleName);
    
                        if (role == null)
                        {
                            throw new NotFoundException($"Role not found: {roleName}");
                        }
    
                        newUser.RoleId = role.Id;
                        newUser.Role = role;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid role: {request.Role}. Valid roles are: {string.Join(", ", Enum.GetNames(typeof(Role)))}");
                    }
                    
                    // 4. Set default values
                    newUser.IsActive = true;
                    
                    // 5. Validate required fields
                    if (string.IsNullOrEmpty(newUser.Name))
                        throw new ArgumentException("Name is required");
                    if (string.IsNullOrEmpty(newUser.Email))
                        throw new ArgumentException("Email is required");
                    if (string.IsNullOrEmpty(newUser.PhoneNumber))
                        throw new ArgumentException("Phone number is required");
                    if (string.IsNullOrEmpty(newUser.Address))
                        throw new ArgumentException("Address is required");

                    Console.WriteLine($"Creating user: {newUser.Email} with role: {newUser.Role}");

                    // Add the new user
                    await _unitOfWork.GetRepository<User>().InsertAsync(newUser);

                    return _mapper.Map<CreateUserResponse>(newUser);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                _logger.LogError("An error occurred while creating: " + ex.Message);
                throw;
            }
        }

        public async Task<GetUserResponse> GetUserById(Guid id)
        {
            try
            {
                var user = await _unitOfWork.GetRepository<User>()
                    .FirstOrDefaultAsync(
                        predicate: u => u.Id == id && u.IsActive,
                        include: u => u.Include(x => x.Role)
                    );

                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }

                return _mapper.Map<GetUserResponse>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving user by ID: {Id}. Error: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<UpdateUserResponse> UpdateUser(Guid id, UpdateUserRequest request)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    if (request == null)
                    {
                        throw new ArgumentNullException(nameof(request), "Request cannot be null.");
                    }

                    var existingUser = await _unitOfWork.GetRepository<User>()
                        .FirstOrDefaultAsync(
                            predicate: u => u.Id == id && u.IsActive,
                            include: u => u.Include(x => x.Role));

                    if (existingUser == null)
                    {
                        throw new NotFoundException("User not found.");
                    }

                    // Check if email is being changed and if it's already taken
                    if (existingUser.Email != request.Email)
                    {
                        var emailExists = await _unitOfWork.GetRepository<User>()
                            .FirstOrDefaultAsync(
                                predicate: u => u.Email == request.Email && u.IsActive && u.Id != id
                            );

                        if (emailExists != null)
                        {
                            throw new BadRequestException("A user with this email already exists.");
                        }
                    }

                    // Map the request to the existing user
                    _mapper.Map(request, existingUser);

                    // Update the user
                    _unitOfWork.GetRepository<User>().UpdateAsync(existingUser);

                    return _mapper.Map<UpdateUserResponse>(existingUser);
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteUser(Guid id)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existingUser = await _unitOfWork.GetRepository<User>()
                        .FirstOrDefaultAsync(
                            predicate: u => u.Id == id && u.IsActive,
                    include: u => u.Include(x => x.Role)
                        );

                    if (existingUser == null)
                    {
                        throw new NotFoundException("User not found.");
                    }

                    // Soft delete by setting IsActive to false
                    existingUser.IsActive = false;
                    _unitOfWork.GetRepository<User>().UpdateAsync(existingUser);

                    return true;
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<GetUserResponse> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _unitOfWork.GetRepository<User>()
                    .FirstOrDefaultAsync(
                        predicate: u => u.Email == email && u.IsActive,
                        include: u => u.Include(x => x.Role)
                    );

                if (user == null)
                {
                    return null; // Don't throw exception for security reasons
                }

                return _mapper.Map<GetUserResponse>(user);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdatePasswordAsync(Guid userId, string newPassword)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existingUser = await _unitOfWork.GetRepository<User>()
                        .FirstOrDefaultAsync(
                            predicate: u => u.Id == userId && u.IsActive,
                            include: u => u.Include(x => x.Role)
                        );

                    if (existingUser == null)
                    {
                        throw new InvalidOperationException("User not found.");
                    }

                    // Hash the new password before saving
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

                    _unitOfWork.GetRepository<User>().UpdateAsync(existingUser);

                    return true;
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UpdateUserResponse> SetUserRole(Guid userId, SetUserRoleRequest request)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    if (request == null)
                    {
                        throw new ArgumentNullException(nameof(request), "Request cannot be null.");
                    }

                    var existingUser = await _unitOfWork.GetRepository<User>()
                        .FirstOrDefaultAsync(
                            predicate: u => u.Id == userId && u.IsActive
                        );

                    if (existingUser == null)
                    {
                        throw new InvalidOperationException("User not found.");
                    }

                    // Parse and set the role
                    if (Enum.TryParse<RoleName>(request.Role, true, out var roleName))
                    {
                        var role = await _unitOfWork.GetRepository<Role>()
                            .FirstOrDefaultAsync(predicate: r => r.Name == roleName);
    
                        if (role == null)
                        {
                            throw new NotFoundException($"Role not found: {roleName}");
                        }
    
                        existingUser.RoleId = role.Id;
                        existingUser.Role = role;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid role: {request.Role}. Valid roles are: {string.Join(", ", Enum.GetNames(typeof(RoleName)))}");
                    }

                    // Update the user
                    _unitOfWork.GetRepository<User>().UpdateAsync(existingUser);

                    return _mapper.Map<UpdateUserResponse>(existingUser);
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user role: {ex.Message}", ex);
            }
        }

        public async Task<UpdateUserResponse> UpdateProfile(Guid userId, UpdateProfileRequest request)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    if (request == null)
                    {
                        throw new ArgumentNullException(nameof(request), "Request cannot be null.");
                    }

                    var existingUser = await _unitOfWork.GetRepository<User>()
                        .FirstOrDefaultAsync(
                            predicate: u => u.Id == userId && u.IsActive,
                            include: u => u.Include(x => x.Role)
                        );

                    if (existingUser == null)
                    {
                        throw new InvalidOperationException("User not found.");
                    }

                    // Check if email is being changed and if it's already taken
                    if (existingUser.Email != request.Email)
                    {
                        var emailExists = await _unitOfWork.GetRepository<User>()
                            .FirstOrDefaultAsync(
                                predicate: u => u.Email == request.Email && u.IsActive && u.Id != userId
                            );

                        if (emailExists != null)
                        {
                            throw new InvalidOperationException("A user with this email already exists.");
                        }
                    }

                    // Update profile fields (not role - that's handled separately)
                    existingUser.Name = request.Name;
                    existingUser.Email = request.Email;
                    existingUser.PhoneNumber = request.PhoneNumber;
                    existingUser.Address = request.Address;

                    // Update the user
                    _unitOfWork.GetRepository<User>().UpdateAsync(existingUser);

                    return _mapper.Map<UpdateUserResponse>(existingUser);
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user profile: {ex.Message}", ex);
            }
        }

        public async Task<UpdateUserResponse> UpdateAvatar(Guid userId, UpdateAvatarRequest request)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    if (request == null)
                    {
                        throw new ArgumentNullException(nameof(request), "Request cannot be null.");
                    }

                    var existingUser = await _unitOfWork.GetRepository<User>()
                        .FirstOrDefaultAsync(
                            predicate: u => u.Id == userId && u.IsActive,
                            include: u => u.Include(x => x.Role)
                        );

                    if (existingUser == null)
                    {
                        throw new InvalidOperationException("User not found.");
                    }

                    // Update avatar URL
                    existingUser.AvatarUrl = request.AvatarUrl;

                    // Update the user
                    _unitOfWork.GetRepository<User>().UpdateAsync(existingUser);

                    return _mapper.Map<UpdateUserResponse>(existingUser);
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user avatar: {ex.Message}", ex);
            }
        }

        public async Task<UserProfileResponse> GetUserProfile(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.GetRepository<User>()
                    .FirstOrDefaultAsync(
                        predicate: u => u.Id == userId && u.IsActive,
                        include: u => u
                            .Include(x => x.Posts)
                            .Include(x => x.STITests)
                            .Include(x => x.Role)
                    );

                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }

                // Get additional statistics
                var appointmentCount = await _unitOfWork.GetRepository<Appointment>()
                    .CountAsync(predicate: a => (a.CustomerId == userId || a.ConsultantId == userId) && a.Status != AppointmentStatus.Cancelled);

                // Map to profile response
                var profile = _mapper.Map<UserProfileResponse>(user);
                profile.TotalPosts = user.Posts?.Count ?? 0;
                profile.TotalSTITests = user.STITests?.Count ?? 0;
                profile.TotalAppointments = appointmentCount;

                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving user profile for ID: {UserId}. Error: {Message}", userId, ex.Message);
                throw;
            }
        }

        public async Task<UserProfileResponse> GetCurrentUserProfile(Guid currentUserId)
        {
            try
            {
                // This method can include additional logic specific to the current user
                // For now, it's the same as GetUserProfile but can be extended
                return await GetUserProfile(currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving current user profile for ID: {UserId}. Error: {Message}", currentUserId, ex.Message);
                throw;
            }
        }
    }
}