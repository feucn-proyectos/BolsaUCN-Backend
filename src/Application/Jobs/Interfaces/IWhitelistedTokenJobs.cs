namespace backend.src.Application.Jobs.Interfaces
{
    public interface IWhitelistedTokenJobs
    {
        /// <summary>
        /// Elimina tokens en la whitelist expirados de la base de datos.
        /// </summary>
        Task DeleteExpiredTokensAsync();
    }
}
