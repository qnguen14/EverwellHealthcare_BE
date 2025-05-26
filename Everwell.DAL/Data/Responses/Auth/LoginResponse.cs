using Everwell.DAL.Data.Responses.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Responses.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public GetUserResponse User { get; set; }
        public DateTime Expiration { get; set; }
    }
}
