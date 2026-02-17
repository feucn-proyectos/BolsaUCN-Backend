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

        /// <summary>
        /// Deletes user accounts that have not been confirmed within a certain time frame.
        /// This method would typically query the database for unconfirmed accounts and delete them.
        /// </summary>
        public async Task DeleteUnconfirmedUserAccountsAsync()
        {
            Log.Information("Trabajo programado: Eliminando cuentas de usuario no confirmadas.");
            await _userService.DeleteUnconfirmedUserAccountsAsync();
        }
    }
}
