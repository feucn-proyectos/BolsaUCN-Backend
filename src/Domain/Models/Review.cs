namespace backend.src.Domain.Models
{
    /// <summary>
    /// Representa una reseña bidireccional entre un oferente y un estudiante.
    /// Permite que ambas partes evalúen mutuamente su experiencia laboral.
    /// </summary>
    public class Review : ModelBase
    {
        /// <summary>
        /// Calificación otorgada por el oferente al estudiante (1-6).
        /// Null si el oferente aún no ha completado su evaluación.
        /// </summary>
        public int? RatingForStudent { get; set; }

        /// <summary>
        /// Comentario del oferente sobre el desempeño del estudiante.
        /// </summary>
        public string? CommentForStudent { get; set; }

        /// <summary>
        /// Calificación otorgada por el estudiante al oferente (1-6).
        /// Null si el estudiante aún no ha completado su evaluación.
        /// </summary>
        public int? RatingForOfferor { get; set; }

        /// <summary>
        /// Comentario del estudiante sobre su experiencia con el oferente.
        /// </summary>
        public string? CommentForOfferor { get; set; }

        /// <summary>
        /// Contiene los valores del checklist de evaluación para la reseña.
        /// </summary>
        /// <returns></returns>
        public ReviewChecklistValues ReviewChecklistValues { get; set; } =
            new ReviewChecklistValues();

        /// <summary>
        /// Referencia de navegación al estudiante evaluado.
        /// </summary>
        public User? Student { get; set; }

        /// <summary>
        /// Identificador del estudiante evaluado.
        /// </summary>
        public required int StudentId { get; set; }

        /// <summary>
        /// Referencia de navegación al oferente evaluado.
        /// </summary>
        public User? Offeror { get; set; }

        /// <summary>
        /// Identificador del oferente evaluado.
        /// </summary>
        public required int OfferorId { get; set; }

        /// <summary>
        /// Indica si las evaluaciones hacia el estudiante han sido completadas.
        /// </summary>
        public bool IsReviewForStudentCompleted { get; set; } = false;

        //todo: valor calculado

        /// <summary>
        /// Indica si las evaluaciones hacia el oferente han sido completadas.
        /// </summary>
        public bool IsReviewForOfferorCompleted { get; set; } = false;

        /// <summary>
        /// Indica si ambas partes han completado sus respectivas evaluaciones.
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Indica si la review ha sido cerrada (no se permiten más cambios).
        /// </summary>
        public bool IsClosed { get; set; } = false;

        /// <summary>
        /// Referencia de navegación a la publicación asociada.
        /// </summary>
        public Publication? Publication { get; set; }

        /// <summary>
        /// Identificador de la publicación asociada a esta reseña.
        /// Requerido - cada reseña debe estar asociada a una publicación específica.
        /// </summary>
        public required int PublicationId { get; set; }

        /// <summary>
        /// Indica si la reseña del estudiante ha sido eliminada.
        /// </summary>
        /// <value></value>
        public bool HasReviewForStudentBeenDeleted { get; set; } = false;

        /// <summary>
        /// Indica si la reseña del oferente ha sido eliminada.
        /// </summary>
        /// <value></value>
        public bool HasReviewForOfferorBeenDeleted { get; set; } = false;
    }
}
