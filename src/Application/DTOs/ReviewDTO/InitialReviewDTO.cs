using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.ReviewDTO
{
    public class InitialReviewDTO
    {
        [Required(ErrorMessage = "Se requiere la ID de la publicación.")]
        public int PublicationId { get; set; }

        [Required(ErrorMessage = "Se requiere la ID del oferente.")]
        public int OfferorId { get; set; }

        [Required(ErrorMessage = "Se requiere la ID del estudiante.")]
        public int StudentId { get; set; }

        // reviewWindowEndDate se calculará automáticamente como DateTime.UtcNow.AddDays(14)
    }
}
