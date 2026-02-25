using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.ReviewDTO.CreateReviewDTOs
{
    public class OfferorReviewForApplicantDTO
    {
        [Required(ErrorMessage = "La calificación es obligatoria.")]
        [Range(1f, 6f, ErrorMessage = "La calificación debe estar entre 1 y 6.")]
        public required float Rating { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s]+$",
            ErrorMessage = "El comentario contiene caracteres no permitidos."
        )]
        public required string Comment { get; set; }

        [Required(ErrorMessage = "El campo 'Es respetuoso' es obligatorio.")]
        public required bool IsRespectful { get; set; }

        [Required(ErrorMessage = "El campo 'Es presentable' es obligatorio.")]
        public required bool IsPresentable { get; set; }

        [Required(ErrorMessage = "El campo 'Es puntual' es obligatorio.")]
        public required bool IsOnTime { get; set; }
    }
}
