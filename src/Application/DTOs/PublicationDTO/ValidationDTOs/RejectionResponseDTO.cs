namespace backend.src.Application.DTOs.PublicationDTO.ValidationDTOs
{
    /// <summary>
    /// DTO para representar la acción de validación en una publicación.
    /// </summary>
    public class PublicationApprovalResultDTO
    {
        /// <summary>
        /// ID de la publicación que fue rechazada.
        /// </summary>
        public required int PublicationId { get; set; }

        /// <summary>
        /// Razón de rechazo de la publicación.
        /// </summary>
        public string? RejectionReason { get; set; }
    }
}
