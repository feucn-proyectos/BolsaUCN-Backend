using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.ReviewDTO
{
    /// <summary>
    /// DTO para representar una reseña completa entre oferente y estudiante.
    /// </summary>
    public class ReviewDTO
    {
        /// <summary>
        /// Identificador único de la reseña.
        /// </summary>
        /// <value></value>
        public int IdReview { get; set; }

        /// <summary>
        /// Calificacion otorgada al estudiante.
        /// </summary>
        /// <value></value>
        public int? RatingForStudent { get; set; }

        /// <summary>
        /// Comentario del oferente hacia el estudiante.
        /// </summary>
        /// <value></value>
        public string? CommentForStudent { get; set; }

        /// <summary>
        /// Calificacion otorgada al oferente.
        /// </summary>
        /// <value></value>
        public int? RatingForOfferor { get; set; }

        /// <summary>
        /// Comentario del estudiante hacia el oferente.
        /// </summary>
        /// <value></value>
        public string? CommentForOfferor { get; set; }

        /// <summary>
        /// Variable booleana indicando si el estudiante llego a tiempo a su lugar de trabajo.
        /// </summary>
        /// <value></value>
        public bool AtTime { get; set; }

        /// <summary>
        /// Variable booleana indicando si el estudiante tuvo una buena presentación en la realización del trabajo.
        /// </summary>
        /// <value></value>
        public bool GoodPresentation { get; set; }

        /// <summary>
        /// Variable booleana indicando si el estudiante mostró respeto hacia el oferente.
        /// </summary>
        /// <value></value>
        public bool StudentHasRespectOfferor { get; set; }

        /// <summary>
        /// Fecha y hora de finalización de la ventana de revisión.
        /// </summary>
        /// <value></value>
        public DateTime ReviewWindowEndDate { get; set; }

        /// <summary>
        /// Identificador del estudiante.
        /// </summary>
        /// <value></value>
        public int IdStudent { get; set; }

        /// <summary>
        /// Identificador del oferente.
        /// </summary>
        /// <value></value>
        public int IdOfferor { get; set; }

        /// <summary>
        /// Identificador de la publicación asociada a la reseña. No puede ser nulo.
        /// </summary>
        /// <value></value>
        public required int IdPublication { get; set; }

        /// <summary>
        /// Indica si la reseña hacia el oferente ha sido eliminada.
        /// </summary>
        /// <value></value>
        public bool HasReviewForOfferorBeenDeleted { get; set; } = false;

        /// <summary>
        /// Indica si la reseña hacia el estudiante ha sido eliminada.
        /// </summary>
        /// <value></value>
        public bool HasReviewForStudentBeenDeleted { get; set; } = false;
        public bool IsComplete { get; set; } = false;
    }
}
