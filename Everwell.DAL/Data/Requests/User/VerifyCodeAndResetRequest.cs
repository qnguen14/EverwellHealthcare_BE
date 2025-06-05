using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Requests.User
{
    public class VerifyCodeAndResetRequest
    {
        public string Code { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
