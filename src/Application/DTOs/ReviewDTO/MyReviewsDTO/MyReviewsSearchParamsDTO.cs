using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO
{
    public class MyReviewsSearchParamsDTO
    {
        [StringLength(
            100,
            ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres."
        )]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s]+$",
            ErrorMessage = "El término de búsqueda solo puede contener letras, números y espacios."
        )]
        public string? PublicationTitle { get; set; }

        [RegularExpression(
            "Pendiente|Completada|Cerrada",
            ErrorMessage = "Estado de reseña inválido. Debe ser 'Pendiente', 'Completada' o 'Cerrada'."
        )]
        public string? ReviewStatus { get; set; }

        [RegularExpression(
            "asc|desc",
            ErrorMessage = "Orden de ordenamiento inválido. Debe ser 'asc' o 'desc'."
        )]
        public string? SortOrder { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Número de página debe ser al menos 1.")]
        public int PageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "Tamaño de página debe estar entre 1 y 100.")]
        public int? PageSize { get; set; }
    }
}
