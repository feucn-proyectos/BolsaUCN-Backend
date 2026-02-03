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
        [Range(0, 1000000, ErrorMessage = "La remuneración debe estar entre $0 y $1.000.000")]
        public int? Remuneration { get; set; }

        [Required(ErrorMessage = "El tipo de oferta es obligatorio")]
        [RegularExpression(
            "^(Trabajo|Voluntariado)$",
            ErrorMessage = "El tipo de oferta debe ser 'Trabajo' o 'Voluntariado'"
        )]
        public required string OfferType { get; set; }

        [StringLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string? Location { get; set; }

        [StringLength(1000, ErrorMessage = "Los requisitos no pueden exceder 1000 caracteres")]
        public string? Requirements { get; set; }

        [StringLength(
            200,
            ErrorMessage = "La información de contacto no puede exceder 200 caracteres"
        )]
        [RegularExpression(
            @"^$|^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "El correo electrónico no es válido"
        )]
        public string? AdditionalContactEmail { get; set; }

        [StringLength(15, ErrorMessage = "El número de teléfono no puede exceder 15 caracteres")]
        public string? AdditionalContactPhoneNumber { get; set; }

        [Required(ErrorMessage = "Debe especificar si el CV es obligatorio")]
        public bool IsCvRequired { get; set; }
    }
}
