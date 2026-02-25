namespace backend.src.Application.DTOs.ReviewDTO.ReviewReportDTO
{
    /// <summary>
    /// DTO con el detalle de una review individual para el reporte
    /// </summary>
    public class ReviewDetailDTO
    {
        /// <summary>
        /// ID de la review
        /// </summary>
        public int ReviewId { get; set; }

        /// <summary>
        /// Indica si el usuario es el oferente o el postulante en la publicación asociada a la review
        /// Debido a que el estudiante puede tambien tener el rol de oferente.
        /// </summary>
        public bool IsApplicant { get; set; }

        /// <summary>
        /// Título de la publicación asociada
        /// </summary>
        public string PublicationTitle { get; set; } = string.Empty;

        /// <summary>
        /// Calificación recibida (1-6)
        /// </summary>
        public float? Rating { get; set; }

        /// <summary>
        /// Comentario de la review
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Fecha de creación de la review
        /// </summary>
        public DateTime ReviewDate { get; set; }

        /// <summary>
        /// Nombre del usuario que hizo la review
        /// </summary>
        public string ReviewerName { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el estudiante llegó a tiempo (solo para estudiantes)
        /// </summary>
        public bool? IsOnTime { get; set; }

        /// <summary>
        /// Indica si el estudiante tuvo buena presentación (solo para estudiantes)
        /// </summary>
        public bool? IsPresentable { get; set; }

        /// <summary>
        /// Indica si el estudiante mostró respeto hacia el oferente (solo para estudiantes)
        /// </summary>
        public bool? IsRespectful { get; set; }
    }
}
