using backend.src.Application.Jobs.Interfaces;
using backend.src.Application.Services.Interfaces;
using Hangfire;
using Serilog;

namespace backend.src.Application.Jobs.Implements
{
    [AutomaticRetry(
        Attempts = 10,
        DelaysInSeconds = new int[] { 60, 120, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400 }
    )]
    public class WhitelistedTokenJobs : IWhitelistedTokenJobs
    {
        private readonly ITokenService _tokenService;

        public WhitelistedTokenJobs(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task DeleteExpiredTokensAsync()
        {
            Log.Information("Iniciando proceso de eliminación de tokens whitelisted expirados.");
            await _tokenService.DeleteExpiredTokensAsync();
        }
    }
}
