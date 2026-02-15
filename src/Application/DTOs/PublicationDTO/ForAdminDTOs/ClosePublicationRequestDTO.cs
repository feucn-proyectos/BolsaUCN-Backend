using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs
{
    public class ClosePublicationRequestDTO
    {
        [RegularExpression(
            @"^[a-zA-Z0-9\s.,;:!?'""()\-]*$",
            ErrorMessage = "La razón de cierre solo puede contener letras, números, espacios y signos de puntuación básicos."
        )]
        public required string ClosedByAdminReason { get; set; }
    }
}
