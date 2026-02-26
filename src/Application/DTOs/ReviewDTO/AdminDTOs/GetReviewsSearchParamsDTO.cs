using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.ReviewDTO.AdminDTOs
{
    public class GetReviewsSearchParamsDTO
    {
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s]+$",
            ErrorMessage = "El término de búsqueda solo puede contener letras, números y espacios."
        )]
        public string? SearchTerm { get; set; }

        [RegularExpression(
            "Pendiente|OferenteEvaluoEstudiante|EstudianteEvaluoOferente|Completada|Cerrada",
            ErrorMessage = "Estado de reseña inválido. Debe ser 'Pendiente', 'OferenteEvaluoEstudiante', 'EstudianteEvaluoOferente', 'Completada' o 'Cerrada'."
        )]
        public string? FilterByReviewStatus { get; set; }

        [RegularExpression(
            "JobOfferTitle|OpenUntil",
            ErrorMessage = "Campo de ordenamiento inválido. Debe ser 'JobOfferTitle' o 'OpenUntil'."
        )]
        public string? SortBy { get; set; }

        [RegularExpression(
            "asc|desc",
            ErrorMessage = "Orden de ordenamiento inválido. Debe ser 'asc' o 'desc'."
        )]
        public string? SortOrder { get; set; }
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}
