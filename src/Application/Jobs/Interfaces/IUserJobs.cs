namespace backend.src.Application.Jobs.Interfaces
{
    public interface IUserJobs
    {
        /// <summary>
        /// Elimina cuentas de usuario que no han sido confirmadas dentro del período de retención definido en la configuración.
        /// </summary>
        Task DeleteUnconfirmedUserAccountsAsync();

        /// <summary>
        /// Elimina solicitudes de cambio de correo electrónico pendientes que han expirado segun la configuración de tiempo de expiración.
        /// </summary>
        Task ClearExpiredPendingEmailChangeRequestAsync(int userId);
    }
}
