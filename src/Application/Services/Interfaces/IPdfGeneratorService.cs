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
        /// <param name="userId">ID del usuario para generar el reporte</param>
        /// <returns>Array de bytes del PDF generado</returns>
        /// <exception cref="KeyNotFoundException">Si no se encuentra el usuario</exception>
        Task<byte[]> GenerateUserReviewsPdfAsync(int userId);

        /// <summary>
        /// Genera un PDF con todas las reviews del sistema.
        /// El PDF incluye un resumen general con estadísticas del sistema,
        /// y el detalle de todas las reviews ordenadas por fecha (más recientes primero).
        /// </summary>
        /// <returns>Array de bytes del PDF generado</returns>
        Task<byte[]> GenerateSystemReviewsPdfAsync();
    }
}
