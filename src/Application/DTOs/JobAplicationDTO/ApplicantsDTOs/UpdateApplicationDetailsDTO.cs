using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs
{
    public class UpdateApplicationDetailsDTO
    {
        [MaxLength(
            1000,
            ErrorMessage = "La carta de presentación no puede exceder los 1000 caracteres."
        )]
        [RegularExpression(
            @"^[^<>]*$",
            ErrorMessage = "La carta de presentación contiene caracteres no permitidos."
        )]
        public string? CoverLetter { get; set; }
    }
}
