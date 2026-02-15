namespace backend.src.Application.DTOs.BaseResponse
{
    /// <summary>
    /// Clase que representa los detalles de un error.
    /// </summary>
    /// <param name="message">Mensaje de error.</param>
    /// <param name="details">Detalles adicionales del error (opcional).</param>
    public class ErrorDetail(string message, string? details = null)
    {
        /// <summary>
        /// Mensaje principal del error.
        /// </summary>
        public string Message { get; set; } = message;

        /// <summary>
        /// Detalles adicionales del error (opcional).
        /// </summary>
        public string? Details { get; set; } = details;
    }
}
