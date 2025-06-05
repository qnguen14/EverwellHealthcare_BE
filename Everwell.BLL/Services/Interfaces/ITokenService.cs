using System;

namespace Everwell.BLL.Services.Interfaces
{
    public interface ITokenService
    {
        string GeneratePasswordResetCode(Guid userId);
        bool ValidatePasswordResetCode(string code, string email, out Guid userId);
    }
}
