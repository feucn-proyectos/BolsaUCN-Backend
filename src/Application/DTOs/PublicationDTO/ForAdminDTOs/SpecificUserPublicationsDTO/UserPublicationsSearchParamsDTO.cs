using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.SpecificUserPublicationsDTO
{
    public class UserPublicationsSearchParamsDTO
    {
        [StringLength(
            100,
            ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres."
        )]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s]+$",
            ErrorMessage = "El término de búsqueda solo puede contener letras, números y espacios."
        )]
        public string? SearchByTitle { get; set; }

        [RegularExpression(
            "Pendiente|Aceptada|Rechazada",
            ErrorMessage = "Estado de publicación inválido. Debe ser 'Pendiente', 'Aceptada' o 'Rechazada'."
        )]
        public string? FilterByPublicationStatus { get; set; }

        [RegularExpression(
            "Trabajo|Voluntariado",
            ErrorMessage = "Tipo de oferta inválido. Debe ser 'Trabajo' o 'Voluntariado'."
        )]
        public string? FilterByOfferType { get; set; }

        [RegularExpression(
            "Oferta|CompraVenta",
            ErrorMessage = "Tipo de publicación inválido. Debe ser 'Oferta' o 'CompraVenta'."
        )]
        public string? FilterByPublicationType { get; set; }

        [RegularExpression(
            "Title|CreatedAt",
            ErrorMessage = "Campo de ordenamiento inválido. Debe ser 'Title' o 'CreatedAt'."
        )]
        public string? SortBy { get; set; }

        [RegularExpression(
            "asc|desc",
            ErrorMessage = "El orden de clasificación es inválido. Debe ser 'asc' o 'desc'."
        )]
        public string? SortOrder { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser mayor que 0.")]
        public int PageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
        public int? PageSize { get; set; }
    }
}
