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

namespace Everwell.BLL.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly EverwellDbContext _context;

        public UserService(EverwellDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<CreateUserResponse>> GetUsers()
        {
            try
            {
                var users = await _context.Set<User>().ToListAsync();

                return users.Select(u => new CreateUserResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    Password = u.Password,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive
                });

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }


        public async Task<CreateUserResponse> CreateUser(CreateUserRequest request)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("A user with the same email already exists.");
                }

                var newUser = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    Password = request.Password,
                    Role = Enum.Parse<Role>(request.Role),
                    IsActive = true
                };
                _context.Set<User>().Add(newUser);
                await _context.SaveChangesAsync();

                return new CreateUserResponse
                {
                    Id = newUser.Id,
                    Name = newUser.Name,
                    Email = newUser.Email,
                    PhoneNumber = newUser.PhoneNumber,
                    Address = newUser.Address,
                    Password = newUser.Password,
                    Role = newUser.Role.ToString(),
                    IsActive = newUser.IsActive
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
