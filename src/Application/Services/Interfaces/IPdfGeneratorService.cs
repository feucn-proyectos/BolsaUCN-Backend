namespace backend.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de generación de PDFs
    /// </summary>
    public interface IPdfGeneratorService
    {
        /// <summary>
        /// Genera un PDF con todas las reviews del usuario especificado.
        /// El PDF incluye un resumen con el promedio de calificación y el total de reviews,
        /// así como el detalle de cada review individual.
        /// </summary>
        /// <param name="requestingUserId">ID del usuario que solicita el reporte</param>
        /// <param name="targetUserId">ID del usuario objetivo (opcional, si es null se genera para el usuario solicitante)</param>
        /// <returns>Array de bytes del PDF generado</returns>
        /// <exception cref="KeyNotFoundException">Si no se encuentra el usuario solicitante o objetivo</exception>
        Task<byte[]> GenerateUserReviewsPdfAsync(int requestingUserId, int? targetUserId = null);

        /// <summary>
        /// Genera un PDF con todas las reviews del sistema.
        /// El PDF incluye un resumen general con estadísticas del sistema,
        /// y el detalle de todas las reviews ordenadas por fecha (más recientes primero).
        /// </summary>
        /// <returns>Array de bytes del PDF generado</returns>
        Task<byte[]> GenerateSystemReviewsPdfAsync(int requestingUserId);
    }
}
