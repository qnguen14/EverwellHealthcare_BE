using Everwell.DAL.Data.Requests.Auth;
using Everwell.DAL.Data.Responses.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest request);
    }
}
