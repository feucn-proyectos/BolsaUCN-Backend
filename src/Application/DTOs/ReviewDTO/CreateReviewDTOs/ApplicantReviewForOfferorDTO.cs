using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.ReviewDTO.CreateReviewDTOs
{
    public class ApplicantReviewForOfferorDTO
    {
        [Required]
        [Range(1f, 6f, ErrorMessage = "La calificación debe estar entre 1 y 6.")]
        public required float Rating { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s.,;:!?'""()\-]*$",
            ErrorMessage = "El comentario contiene caracteres no permitidos."
        )]
        public required string Comment { get; set; }
    }
}
