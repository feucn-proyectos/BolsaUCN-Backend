using System.ComponentModel.DataAnnotations;
using backend.src.Application.Validators;

namespace backend.src.Application.DTOs.PublicationDTO.ValidationDTOs
{
    public class AppealRejectionDTO
    {
        // Atributos generales
        [StringLength(
            200,
            MinimumLength = 5,
            ErrorMessage = "El título debe tener entre 5 y 200 caracteres"
        )]
        public string? Tittle { get; set; }

        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres"
        )]
        public string? Description { get; set; }

        [FutureDate(ErrorMessage = "La fecha de finalización debe ser en el futuro")]
        public DateTime? EndDate { get; set; }

        [StringLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string? Location { get; set; }

        [StringLength(
            200,
            ErrorMessage = "La información de contacto no puede exceder 200 caracteres"
        )]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string? AdditionalContactEmail { get; set; }

        [StringLength(15, ErrorMessage = "El número de contacto no puede exceder 200 caracteres")]
        public string? AdditionalContactPhoneNumber { get; set; }
        public DateTime? ApplicationDeadline { get; set; }

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "El número de postulantes requeridos debe ser un número positivo."
        )]
        public int? RequiredApplicants { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La remuneración debe estar entre $0 y $1.000.000")]
        public int? Remuneration { get; set; }

        [RegularExpression(
            "^(Trabajo|Voluntariado)$",
            ErrorMessage = "El tipo de oferta debe ser 'Trabajo' o 'Voluntariado'"
        )]
        public string? OfferType { get; set; }
        public bool? IsCvRequired { get; set; }

        // Atributos específicos de compra/venta
        [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres")]
        public string? Category { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El precio debe estar entre $0 y $100.000.000")]
        public int? Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un número positivo.")]
        public int? Quantity { get; set; }

        [RegularExpression(
            @"^(Disponible|Vendido)$",
            ErrorMessage = "El tipo de disponibilidad no es válido."
        )]
        public string? Availability { get; set; }

        [RegularExpression(
            @"^(Nuevo|ComoNuevo|Usado|NoAplica)$",
            ErrorMessage = "La condición no es válida."
        )]
        public string? Condition { get; set; }

        //public List<IFormFile>? Images { get; set; }
    }
}
