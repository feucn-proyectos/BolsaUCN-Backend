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
    }
}
