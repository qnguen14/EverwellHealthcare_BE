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
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.Implements
{
    public class UserService : BaseService<UserService>, IUserService
    {
        public UserService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<UserService> logger, IMapper mapper)
            : base(unitOfWork, logger, mapper)
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
                        predicate: u => u.IsActive
                    );

                if (users == null || !users.Any())
                {
                    throw new DirectoryNotFoundException("No active users found.");
                }

                return _mapper.Map<IEnumerable<CreateUserResponse>>(users);
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
                        throw new InvalidOperationException("A user with this email already exists.");
                    }

                    var newUser = _mapper.Map<User>(request);

                    // Add the new user
                    await _unitOfWork.GetRepository<User>().InsertAsync(newUser);

                    return _mapper.Map<CreateUserResponse>(newUser);
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}