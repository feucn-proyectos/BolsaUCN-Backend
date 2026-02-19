namespace backend.src.Application.Jobs.Interfaces
{
    public interface IOfferJobs
    {
        /// <summary>
        /// Cierra el proceso de postulación para una oferta, estableciendo el estado de la oferta como "Cerrada para Postulaciones" y evitando nuevas postulaciones.
        /// En esencia, solo actualiza el estado.
        /// </summary>
        /// <param name="offerId"></param>
        /// <returns></returns>
        Task SetAsCloseForApplicationsAsync(int offerId);

        /// <summary>
        /// Marca la oferta como "Trabajo Finalizado", indicando que el proceso de trabajo ha concluido.
        /// Actualiza el estado de la oferta a "Trabajo Finalizado", lo que desencadena acciones adicionales con las calificaciones.
        /// </summary>
        /// <param name="offerId"></param>
        /// <returns></returns>
        Task SetAsCompleteAndInitializeReviewsAsync(int offerId);

        /// <summary>
        /// Marca la oferta como "Finalizada" y cierra las reseñas asociadas, estableciendo el estado de la oferta como "Finalizada".
        /// Este método se ejecuta después de un período de tiempo definido desde la finalización del trabajo para garantizar que los usuarios tengan tiempo suficiente para dejar sus reseñas antes de cerrarlas.
        /// </summary>
        /// <param name="offerId"></param>
        /// <returns></returns>
        Task SetAsFinalizedAndCloseReviewsAsync(int offerId);
    }
}
