namespace backend.src.Application.DTOs.ReviewDTO.ReviewReportDTO
{
    /// <summary>
    /// DTO que contiene toda la información necesaria para generar un reporte PDF de reviews de un usuario
    /// </summary>
    public class ReviewReportDTO
    {
        /// <summary>
        /// Nombre del usuario
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// Promedio de calificación del usuario (obtenido de GeneralUser.Rating)
        /// </summary>
        public float? AverageRating { get; set; }

        /// <summary>
        /// Total de reviews del usuario
        /// </summary>
        public int TotalReviews { get; set; }

        /// <summary>
        /// Fecha y hora de generación del reporte
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Lista de reviews detalladas
        /// </summary>
        public List<ReviewDetailDTO> Reviews { get; set; } = [];
    }
}
