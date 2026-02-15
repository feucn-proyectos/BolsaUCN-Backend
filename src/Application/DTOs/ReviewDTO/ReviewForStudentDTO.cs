using System.ComponentModel.DataAnnotations;
using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.ReviewDTO
{
    public class ReviewForStudentDTO
    {
        [Required(ErrorMessage = "El rating es obligatorio.")]
        [Range(
            1,
            6,
            ErrorMessage = "El rating debe tener entre 1 y 6 estrellas como valores enteros."
        )]
        public int RatingForStudent { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(320, ErrorMessage = "El comentario no puede exceder los 320 caracteres.")]
        public string CommentForStudent { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de envío es obligatoria.")]
        public DateTime SendedAt { get; set; }
        public required bool AtTime { get; set; }
        public required bool GoodPresentation { get; set; }
        public required bool StudentHasRespectOfferor { get; set; }
        public required int ReviewId { get; set; }
    }
}
