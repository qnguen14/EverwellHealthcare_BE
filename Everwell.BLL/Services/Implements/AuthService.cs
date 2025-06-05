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

namespace Everwell.BLL.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TokenProvider _tokenProvider;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(IUnitOfWork<EverwellDbContext> unitOfWork, TokenProvider tokenProvider, IConfiguration configuration, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenProvider = tokenProvider;
            _configuration = configuration;
            _mapper = mapper;
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
                var user = await _unitOfWork.GetRepository<User>().FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, null, null);
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
                var userResponse = _mapper.Map<GetUserResponse>(user);

                // Return response
                return new LoginResponse
                {
                    Token = token,
                    User = userResponse,
                    Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationInMinutes"]))
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