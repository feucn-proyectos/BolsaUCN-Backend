namespace backend.src.Application.DTOs.ReviewDTO.ReviewReport
{
    /// <summary>
    /// DTO con el detalle de una review para el reporte del sistema
    /// Incluye información tanto del estudiante como del oferente
    /// </summary>
    public class SystemReviewDetailDTO
    {
        /// <summary>
        /// ID de la review
        /// </summary>
        public int ReviewId { get; set; }

        /// <summary>
        /// Título de la publicación asociada
        /// </summary>
        public string PublicationTitle { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del estudiante
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del oferente
        /// </summary>
        public string OfferorName { get; set; } = string.Empty;

        /// <summary>
        /// Calificación que recibió el estudiante
        /// </summary>
        public int? RatingForStudent { get; set; }

        /// <summary>
        /// Comentario para el estudiante
        /// </summary>
        public string? CommentForStudent { get; set; }

        /// <summary>
        /// Calificación que recibió el oferente
        /// </summary>
        public int? RatingForOfferor { get; set; }

        /// <summary>
        /// Comentario para el oferente
        /// </summary>
        public string? CommentForOfferor { get; set; }

        /// <summary>
        /// Fecha de creación de la review
        /// </summary>
        public DateTime ReviewDate { get; set; }

        /// <summary>
        /// Indica si el estudiante llegó a tiempo
        /// </summary>
        public bool? AtTime { get; set; }

        /// <summary>
        /// Indica si el estudiante tuvo buena presentación
        /// </summary>
        public bool? GoodPresentation { get; set; }

        /// <summary>
        /// Indica si el estudiante mostró respeto hacia el oferente
        /// </summary>
        public bool? StudentHasRespectOfferor { get; set; }

        /// <summary>
        /// Indica si la review está completada
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Indica si la review está cerrada
        /// </summary>
        public bool IsClosed { get; set; }
    }
}
