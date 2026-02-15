using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO.ValidationDTOs
{
    /// <summary>
    /// DTO para representar la acción de validación en una publicación.
    /// </summary>
    public class ApprovalActionDTO
    {
        /// <summary>
        /// Acción a realizar (aceptar o rechazar).
        /// </summary>
        [Required(ErrorMessage = "La acción es obligatoria")]
        [RegularExpression(
            "^(publish|reject)$",
            ErrorMessage = "La acción debe ser 'publish' o 'reject'"
        )]
        public required string Action { get; set; }

        [StringLength(
            500,
            ErrorMessage = "La razón de rechazo no puede exceder los 500 caracteres"
        )]
        [RegularExpression(
            @"^[a-zA-Z0-9\s.,;:!?'""()\-]*$",
            ErrorMessage = "La razón de rechazo solo puede contener caracteres alfanuméricos y signos de puntuación básicos"
        )]
        public string? RejectionReason { get; set; }
    }
}
