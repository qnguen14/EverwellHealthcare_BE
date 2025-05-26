using Everwell.BLL.Infrastructure;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Auth;
using Everwell.DAL.Data.Responses.Auth;
using Everwell.DAL.Data.Responses.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Everwell.BLL.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly EverwellDbContext _context;
        private readonly TokenProvider _tokenProvider;
        private readonly IConfiguration _configuration;

        public AuthService(EverwellDbContext context, TokenProvider tokenProvider, IConfiguration configuration)
        {
            _context = context;
            _tokenProvider = tokenProvider;
            _configuration = configuration;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new ArgumentException("Email and password must be provided.");
                }

                // Fetch user from the database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                // Verify password (replace with actual password hashing logic)
                if (!VerifyPassword(request.Password, user.Password))
                {
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                // Generate token (replace with actual token generation logic, e.g., JWT)
                var token = _tokenProvider.Create(user);

                // Map user to GetUserResponse
                var userResponse = new GetUserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Role = user.Role.ToString()
                };

                // Return response
                return new LoginResponse
                {
                    Token = token,
                    User = userResponse,
                    Expiration = DateTime.UtcNow.AddMinutes(60)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred during login: {ex.Message}", ex);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            // Implement password verification logic here
            // This is a placeholder implementation
            return password == hashedPassword;
            //return BCrypt.Net.BCrypt.Verify(password, hashedPassword); // to do
        }
    }
}
