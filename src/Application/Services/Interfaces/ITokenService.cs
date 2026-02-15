using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user, IList<string> roleNames, bool rememberMe);
        Task<bool> AddToWhitelistAsync(Whitelist token);
        Task<bool> RevokeAllActiveTokensAsync(int userId);
    }
}
