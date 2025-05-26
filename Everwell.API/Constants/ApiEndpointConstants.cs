namespace Everwell.API.Constants
{
    public class ApiEndpointConstants
    {
        static ApiEndpointConstants() {  }

        public const string RootEndpoint = "/api";
        public const string ApiVersion = "/v1";
        public const string ApiEndpoint = RootEndpoint + ApiVersion;

        public static class Auth
        {
            public const string AuthEndpoint = ApiEndpoint + "/auth";
            public const string LoginEndpoint = ApiEndpoint + "/login";
            public const string RegisterEndpoint = ApiEndpoint + "/register"; // to do
            public const string ChangePasswordEndpoint = ApiEndpoint + "/changepassword"; // to do
            public const string RefreshTokenEndpoint = ApiEndpoint + "/refreshtoken"; // to do
            public const string LogoutEndpoint = ApiEndpoint + "/logout"; // to do
        }

        public static class User
        {
            public const string UserEndpoint = ApiEndpoint + "/user";
            public const string GetUserEndpoint = UserEndpoint + "/{id}"; // to do
            public const string GetAllUsersEndpoint = UserEndpoint + "/getall"; 
            public const string CreateUserEndpoint = UserEndpoint + "/create"; 
            public const string UpdateUserEndpoint = UserEndpoint + "/update"; // to do
            public const string DeleteUserEndpoint = UserEndpoint + "/delete"; // to do
        }
    }
}
