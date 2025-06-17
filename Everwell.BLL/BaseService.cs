using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Everwell.DAL.Data.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Everwell.BLL.Services
{
    public abstract class BaseService<T> where T : class
    {
        protected IUnitOfWork<EverwellDbContext> _unitOfWork;
        protected ILogger<T> _logger;
        protected IMapper _mapper;
        protected IHttpContextAccessor _httpContextAccessor;

        public BaseService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<T> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        protected Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedException("User ID not found in token");
            }
            return userId;
        }

        protected string GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                ?? throw new UnauthorizedException("User email not found in token");
        }

        protected string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value
                ?? throw new UnauthorizedException("User role not found in token");
        }
    }
}