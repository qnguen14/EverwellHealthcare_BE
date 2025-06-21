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
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsUnauthorized { get; set; }
    }
}
