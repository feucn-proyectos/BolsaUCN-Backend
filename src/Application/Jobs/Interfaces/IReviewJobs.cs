namespace backend.src.Application.Jobs.Interfaces
{
    public interface IReviewJobs
    {
        /// <summary>
        /// Trabajo para crear reseñas iniciales después de que una publicación es marcada como "completada".
        /// Este trabajo se programa como "Fire and Forget" automaticamente por la oferta.
        /// </summary>
        /// <returns></returns>
        Task CreateInitialReviewAsync(int offerId);
    }
}
