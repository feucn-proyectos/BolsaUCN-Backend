using Serilog;

namespace backend.src.Application.Jobs.Implements
{
    using backend.src.Application.Jobs.Interfaces;
    using backend.src.Application.Services.Interfaces;
    using Hangfire;

    [AutomaticRetry(
        Attempts = 10,
        DelaysInSeconds = new int[] { 60, 120, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400 }
    )]
    public class UserJobs : IUserJobs
    {
        private readonly IUserService _userService;

        public UserJobs(IUserService userService)
        {
            _userService = userService;
        }

        public async Task DeleteUnconfirmedUserAccountsAsync()
        {
            Log.Information("Trabajo programado: Eliminando cuentas de usuario no confirmadas.");
            await _userService.DeleteUnconfirmedUserAccountsAsync();
        }

        public async Task ClearExpiredPendingEmailChangeRequestAsync(int userId)
        {
            Log.Information(
                "Trabajo programado: Limpiando solicitudes de cambio de correo electrónico no confirmadas para el usuario con ID {UserId}.",
                userId
            );
            await _userService.ClearExpiredPendingEmailChangeRequestAsync(userId);
        }
    }
}
