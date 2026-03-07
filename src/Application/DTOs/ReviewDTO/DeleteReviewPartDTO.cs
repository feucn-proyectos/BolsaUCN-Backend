namespace backend.src.Application.DTOs.ReviewDTO
{
    public class DeleteReviewPartDTO
    {
        /// <summary>
        /// Identificador de la reseña a modificar.
        /// </summary>
        /// <value></value>
        public required int ReviewId { get; set; }

        /// <summary>
        /// Indica si se debe eliminar la parte de la reseña del estudiante hacia el oferente.
        /// </summary>
        /// <value></value>
        public bool DeleteReviewForStudent { get; set; } = false;

        /// <summary>
        /// Indica si se debe eliminar la parte de la reseña del oferente hacia el estudiante.
        /// </summary>
        /// <value></value>
        public bool DeleteReviewForOfferor { get; set; } = false;
    }
}
