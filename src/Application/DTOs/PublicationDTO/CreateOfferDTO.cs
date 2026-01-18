using System.ComponentModel.DataAnnotations;
using backend.src.Application.Validators;
using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO para la creación de ofertas laborales o de voluntariado
    /// </summary>
    public class CreateOfferDTO
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(
            200,
            MinimumLength = 5,
            ErrorMessage = "El título debe tener entre 5 y 200 caracteres"
        )]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres"
        )]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de finalización del trabajo es obligatoria")]
        [FutureDate(ErrorMessage = "La fecha de finalización debe ser en el futuro")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "La fecha Inicial de trabajo es obligatoria")]
        [FutureDate(ErrorMessage = "La fecha límite debe ser en el futuro")]
        [CompareDate(
            "EndDate",
            ErrorMessage = "La fecha límite debe ser anterior a la fecha de termino de postulacion"
        )]
        public DateTime ApplicationDeadline { get; set; }

        [Required(ErrorMessage = "La remuneración es obligatoria")]
        [Range(1, 1000000, ErrorMessage = "La remuneración debe estar entre $1 y $1.000.000")]
        public int? Remuneration { get; set; }

        [Required(ErrorMessage = "El tipo de oferta es obligatorio")]
        [Range(0, 1, ErrorMessage = "El Tipo debe ser 1 (Voluntario) o 0 (Oferta)")]
        public OfferTypes OfferType { get; set; }

        [StringLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string? Location { get; set; }

        [StringLength(1000, ErrorMessage = "Los requisitos no pueden exceder 1000 caracteres")]
        public string? Requirements { get; set; }

        [StringLength(
            200,
            ErrorMessage = "La información de contacto no puede exceder 200 caracteres"
        )]
        public string? AdditionalContactInfo { get; set; }

        /* === IMÁGENES (Manejo de imágenes se implemetara despues de la refactorizacion ===
        [MaxLength(10, ErrorMessage = "Máximo 10 imágenes permitidas")]
        public List<IFormFile> Images { get; set; } = [];
        */

        /// <summary>
        /// Indica si el CV es obligatorio para postular a esta oferta
        /// Por defecto es true (obligatorio)
        /// </summary>
        public bool IsCvRequired { get; set; } = true;
    }
}
