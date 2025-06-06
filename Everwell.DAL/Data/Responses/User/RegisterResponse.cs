using Everwell.DAL.Data.Responses.User;

namespace Everwell.DAL.Data.Responses.Auth
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public GetUserResponse? User { get; set; }
    }
}