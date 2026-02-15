using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers
{
    public class ExploreOffersSearchParamsDTO
    {
        [StringLength(
            100,
            ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres."
        )]
        public string? SearchTerm { get; set; }

        [RegularExpression(
            "Trabajo|Voluntariado|Todos",
            ErrorMessage = "FilterBy debe ser 'Trabajo', 'Voluntariado' o 'Todos'."
        )]
        public string? FilterBy { get; set; }

        [RegularExpression(
            "Title|CreatedAt|Remuneration",
            ErrorMessage = "SortBy debe ser 'Title', 'CreatedAt' o 'Remuneration'."
        )]
        public string? SortBy { get; set; }

        [RegularExpression("asc|desc", ErrorMessage = "SortOrder debe ser 'asc' o 'desc'.")]
        public string? SortOrder { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Numero de pagina debe ser al menos 1.")]
        public required int PageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "Tamaño de página debe estar entre 1 y 100.")]
        public int? PageSize { get; set; }
    }
}
