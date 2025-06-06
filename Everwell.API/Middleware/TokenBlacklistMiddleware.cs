using Everwell.BLL.Services.Interfaces;

namespace Everwell.API.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public TokenBlacklistMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            
            if (!string.IsNullOrEmpty(token))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                    
                    if (await tokenService.IsTokenBlacklistedAsync(token))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Token has been revoked.");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
} 