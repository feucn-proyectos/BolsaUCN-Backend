using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs.ApplicationsForOfferorDTOs
{
    public class UpdateApplicationStatusDTO
    {
        [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
        [RegularExpression(
            "^(Aceptada|Rechazada|Pendiente)$",
            ErrorMessage = "Estado inválido. Los estados permitidos son: Aceptada, Rechazada, Pendiente."
        )]
        public required string NewStatus { get; set; }
    }
}
