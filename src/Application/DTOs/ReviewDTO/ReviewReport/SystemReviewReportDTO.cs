namespace backend.src.Application.DTOs.ReviewDTO.ReviewReport
{
    /// <summary>
    /// DTO que contiene toda la información necesaria para generar un reporte PDF del sistema completo
    /// </summary>
    public class SystemReviewReportDTO
    {
        /// <summary>
        /// Total de reviews en el sistema
        /// </summary>
        public int TotalReviews { get; set; }

        /// <summary>
        /// Total de usuarios con reviews
        /// </summary>
        public int TotalUsersWithReviews { get; set; }

        /// <summary>
        /// Fecha y hora de generación del reporte
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Lista de reviews detalladas del sistema, ordenadas por fecha
        /// </summary>
        public List<SystemReviewDetailDTO> Reviews { get; set; } = new();
    }
}
