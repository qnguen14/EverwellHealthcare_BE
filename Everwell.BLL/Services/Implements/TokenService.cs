using Everwell.BLL.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Everwell.BLL.Services.Implements
{
    public class TokenService : ITokenService
    {
        private readonly IMemoryCache _cache;

        public TokenService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string GeneratePasswordResetCode(Guid userId)
        {
            // Generate a 6-digit random code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            
            // Store the code in cache with user ID for 15 minutes
            var cacheKey = $"reset_code_{code}";
            var cacheValue = new { UserId = userId, CreatedAt = DateTime.UtcNow };
            
            _cache.Set(cacheKey, cacheValue, TimeSpan.FromMinutes(15));
            
            Console.WriteLine($"Generated reset code {code} for user {userId}");
            return code;
        }

        public bool ValidatePasswordResetCode(string code, string email, out Guid userId)
        {
            userId = Guid.Empty;
            
            try
            {
                var cacheKey = $"reset_code_{code}";
                
                if (_cache.TryGetValue(cacheKey, out dynamic cacheValue))
                {
                    userId = cacheValue.UserId;
                    
                    // Remove the code after use (one-time use)
                    _cache.Remove(cacheKey);
                    
                    Console.WriteLine($"Valid reset code {code} for user {userId}");
                    return true;
                }
                
                Console.WriteLine($"Invalid or expired reset code: {code}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating reset code: {ex.Message}");
                return false;
            }
        }
    }
}
