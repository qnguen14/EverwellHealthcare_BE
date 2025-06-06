using System;

namespace Everwell.BLL.Services.Interfaces
{
    public interface ITokenService
    {
        string GeneratePasswordResetCode(Guid userId);
        bool ValidatePasswordResetCode(string code, string email, out Guid userId);

        // Blacklist token
        Task<bool> BlacklistTokenAsync(string token);
        Task<bool> IsTokenBlacklistedAsync(string token);
        Task CleanupExpiredTokensAsync();
    }
}
