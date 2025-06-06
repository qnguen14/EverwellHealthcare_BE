using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace Everwell.BLL.Services.Implements
{
    public class TokenService : ITokenService
    {
        private readonly IMemoryCache _cache;
        private readonly IUnitOfWork<EverwellDbContext> _unitOfWork;

        public TokenService(IMemoryCache cache, IUnitOfWork<EverwellDbContext> unitOfWork)
        {
            _cache = cache;
            _unitOfWork = unitOfWork;
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

        public async Task<bool> BlacklistTokenAsync(string token)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var tokenHash = ComputeHash(token);
                    var expiresAt = GetTokenExpiration(token);
                    
                    // Ensure DateTime values are UTC
                    var blacklistedToken = new BlacklistedToken
                    {
                        Id = Guid.NewGuid(),
                        TokenHash = tokenHash,
                        BlacklistedAt = DateTime.UtcNow, // Use UTC
                        ExpiresAt = DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc) // Ensure UTC kind
                    };

                    await _unitOfWork.GetRepository<BlacklistedToken>().InsertAsync(blacklistedToken);
                    _cache.Set($"blacklisted_{tokenHash}", true, blacklistedToken.ExpiresAt);

                    Console.WriteLine($"Token blacklisted successfully: {tokenHash.Substring(0, 10)}...");
                    return true;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error blacklisting token: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            try
            {
                var tokenHash = ComputeHash(token);
                
                if (_cache.TryGetValue($"blacklisted_{tokenHash}", out _))
                {
                    return true;
                }

                var blacklistedToken = await _unitOfWork.GetRepository<BlacklistedToken>()
                    .FirstOrDefaultAsync(
                        predicate: bt => bt.TokenHash == tokenHash && bt.ExpiresAt > DateTime.UtcNow,
                        orderBy: null,
                        include: null);

                if (blacklistedToken != null)
                {
                    _cache.Set($"blacklisted_{tokenHash}", true, blacklistedToken.ExpiresAt);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking blacklisted token: {ex.Message}");
                return false;
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var expiredTokens = await _unitOfWork.GetRepository<BlacklistedToken>()
                        .GetListAsync(
                            predicate: bt => bt.ExpiresAt <= DateTime.UtcNow,
                            orderBy: null,
                            include: null,
                            take: null);

                    if (expiredTokens.Any())
                    {
                        foreach (var token in expiredTokens)
                        {
                            _unitOfWork.GetRepository<BlacklistedToken>().DeleteAsync(token);
                        }
                        Console.WriteLine($"Cleaned up {expiredTokens.Count} expired tokens");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up expired tokens: {ex.Message}");
            }
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private DateTime GetTokenExpiration(string token)
        {
            try
            {
                // For now, set a default expiration based on JWT config
                // Return UTC time - PostgreSQL will handle timezone conversion
                return DateTime.UtcNow.AddHours(24); // Adjust based on your JWT expiration
            }
            catch
            {
                return DateTime.UtcNow.AddHours(24);
            }
        }
    }
}