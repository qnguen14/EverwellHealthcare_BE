using AutoMapper;
using Everwell.BLL.Infrastructure;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Auth;
using Everwell.DAL.Data.Responses.Auth;
using Everwell.DAL.Data.Responses.User;
using Everwell.DAL.Repositories.Implements;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using Everwell.DAL.Data.Exceptions;
using Microsoft.Extensions.Logging;
using Everwell.DAL.Data.Requests.User;

namespace Everwell.BLL.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TokenProvider _tokenProvider;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;

        public AuthService(
            IUnitOfWork<EverwellDbContext> unitOfWork, 
            TokenProvider tokenProvider, 
            IConfiguration configuration, 
            IMapper mapper, 
            ILogger<AuthService> logger,
            ITokenService tokenService, 
            IEmailService emailService, 
            IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _tokenProvider = tokenProvider;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
            _tokenService = tokenService;
            _emailService = emailService;
            _userService = userService;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new NotFoundException("Email and password must be provided.");
                }

                Console.WriteLine($"Login attempt for: {request.Email}");

                // Fetch user from the database
                var user = await _unitOfWork.GetRepository<User>()
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, 
                                        null, 
                                        include: u => u.Include(u => u.Role));
                if (user == null)
                {
                    return null;
                }

                Console.WriteLine($"User found: {user.Email}");

                // Verify password
                bool isPasswordValid = VerifyPassword(request.Password, user.Password);
                if (!isPasswordValid)
                {
                    Console.WriteLine("Password verification failed");
                    return new LoginResponse { IsUnauthorized = true };
                }

                Console.WriteLine("Password verified successfully");

                // If password is valid but stored as plain text, upgrade it to BCrypt hash
                if (!IsBCryptHash(user.Password))
                {
                    Console.WriteLine($"Upgrading plain text password to BCrypt hash for user: {user.Email}");
                    try
                    {
                        await _userService.UpdatePasswordAsync(user.Id, request.Password);
                        Console.WriteLine("Password upgraded successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to upgrade password: {ex.Message}");
                        // Don't fail login if password upgrade fails
                    }
                }

                // Generate token
                var token = _tokenProvider.Create(user);

                // Map user to GetUserResponse
                var userResponse = _mapper.Map<GetUserResponse>(user);

                Console.WriteLine($"Login successful for: {user.Email}");

                // Return response
                var response = new LoginResponse
                {
                    Token = token,
                    FullName = userResponse.Name,
                    Email = userResponse.Email,
                    IsUnauthorized = false,
                    Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationInMinutes"]))
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                throw;
            }
        }

        private bool VerifyPassword(string password, string storedPassword)
        {
            try
            {
                Console.WriteLine($"Verifying password for stored password: {storedPassword?.Substring(0, Math.Min(10, storedPassword?.Length ?? 0))}...");
                
                // Check if the stored password is a BCrypt hash
                if (IsBCryptHash(storedPassword))
                {
                    Console.WriteLine("Stored password is BCrypt hash - using BCrypt.Verify");
                    return BCrypt.Net.BCrypt.Verify(password, storedPassword);
                }
                else
                {
                    Console.WriteLine("Stored password appears to be plain text - using direct comparison");
                    // It's likely plain text (old format), do direct comparison
                    return password == storedPassword;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password verification error: {ex.Message}");
                
                // Fallback to plain text comparison for safety
                return password == storedPassword;
            }
        }

        private bool IsBCryptHash(string password)
        {
            // BCrypt hashes start with $2a$, $2b$, $2y$, or $2x$ followed by cost
            return password != null && 
                   password.Length >= 60 && 
                   (password.StartsWith("$2a$") || 
                    password.StartsWith("$2b$") || 
                    password.StartsWith("$2y$") || 
                    password.StartsWith("$2x$"));
        }

        public async Task<bool> SendPasswordResetCodeAsync(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal if email exists or not for security
                    return true;
                }

                // Generate 6-digit code
                var resetCode = _tokenService.GeneratePasswordResetCode(user.Id);
                
                // Send email with code
                await _emailService.SendPasswordResetCodeAsync(user.Email, resetCode, user.Name);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending reset code: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyResetCodeAndResetPasswordAsync(string code, string email, string newPassword)
        {
            try
            {
                if (!_tokenService.ValidatePasswordResetCode(code, email, out Guid userId))
                    return false;

                var user = await _userService.GetUserById(userId);
                if (user == null)
                    return false;

                // Update user password
                await _userService.UpdatePasswordAsync(userId, newPassword);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }

        public async Task<RegisterResponse> Register(RegisterRequest request)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _unitOfWork.GetRepository<User>()
                    .FirstOrDefaultAsync(u => u.Email == request.Email, null, null);
                
                if (existingUser != null)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Email already exists. Please use a different email address."
                    };
                }

                // Create user with Customer role (default role for public registration)
                var createUserRequest = new CreateUserRequest
                {
                    Name = request.Name,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    Password = request.Password,
                    Role = RoleName.Customer.ToString() // Default role for public registration
                };

                // Use the existing UserService to create the user
                var userResponse = await _userService.CreateUser(createUserRequest);

                // Convert CreateUserResponse to GetUserResponse manually (safer approach)
                var getUserResponse = new GetUserResponse
                {
                    Id = userResponse.Id,
                    Name = userResponse.Name,
                    Email = userResponse.Email,
                    PhoneNumber = userResponse.PhoneNumber,
                    Address = userResponse.Address,
                    Role = userResponse.Role,
                    AvatarUrl = null, // Default for new users
                    // IsActive = userResponse.IsActive
                };

                return new RegisterResponse
                {
                    Success = true,
                    Message = "User registered successfully. You can now login with your credentials.",
                    User = getUserResponse
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return new RegisterResponse
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again."
                };
            }
        }

        public async Task<LogoutResponse> Logout(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new LogoutResponse
                    {
                        Success = false,
                        Message = "Token is required for logout."
                    };
                }

                // Remove "Bearer " prefix if present
                if (token.StartsWith("Bearer "))
                {
                    token = token.Substring(7);
                }

                // Blacklist the token
                var blacklisted = await _tokenService.BlacklistTokenAsync(token);

                if (blacklisted)
                {
                    return new LogoutResponse
                    {
                        Success = true,
                        Message = "Logged out successfully."
                    };
                }
                else
                {
                    return new LogoutResponse
                    {
                        Success = false,
                        Message = "Failed to logout. Please try again."
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
                return new LogoutResponse
                {
                    Success = false,
                    Message = "An error occurred during logout."
                };
            }
        }
    }
}