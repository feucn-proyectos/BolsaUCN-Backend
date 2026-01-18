using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user, string roleName, bool rememberMe);
        Task<bool> AddToWhitelistAsync(Whitelist token);
        Task<bool> RevokeAllActiveTokensAsync(int userId);
    }
}
