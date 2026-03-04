using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs
{
    public class EditMyBuySellDetailsDTO
    {
        [StringLength(
            200,
            MinimumLength = 5,
            ErrorMessage = "El título debe tener entre 5 y 200 caracteres"
        )]
        public string? Title { get; set; }

        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres"
        )]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string? Location { get; set; }

        [Range(0, 100000000, ErrorMessage = "El precio debe estar entre $0 y $100.000.000")]
        public int? Price { get; set; }

        [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres")]
        public string? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int? Quantity { get; set; }

        [RegularExpression(
            @"^(Nuevo|ComoNuevo|Usado|NoAplica)$",
            ErrorMessage = "La condición no es válida."
        )]
        public string? Condition { get; set; }

        [StringLength(
            200,
            ErrorMessage = "La información de contacto no puede exceder 200 caracteres"
        )]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string? AdditionalContactEmail { get; set; }

        public bool ShowEmail { get; set; }

        [StringLength(15, ErrorMessage = "El número de contacto no puede exceder 200 caracteres")]
        public string? AdditionalContactPhoneNumber { get; set; }

        public bool ShowPhoneNumber { get; set; }
        public List<string>? ImagesToDelete { get; set; }
        public List<IFormFile>? ImagesToUpload { get; set; }
    }
}
