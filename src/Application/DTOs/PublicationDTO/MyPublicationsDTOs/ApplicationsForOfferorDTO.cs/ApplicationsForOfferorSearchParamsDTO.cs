using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs
{
    public class ApplicationsForOfferorSearchParamsDTO
    {
        [RegularExpression(
            "^(FirstName|ApplicationDate)$",
            ErrorMessage = "Campo de ordenamiento inválido."
        )]
        public string? SortBy { get; set; }

        [RegularExpression("^(asc|desc)$", ErrorMessage = "Orden inválido.")]
        public string? SortOrder { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Numero de pagina debe ser al menos 1.")]
        public required int PageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "Tamaño de página debe estar entre 1 y 100.")]
        public int? PageSize { get; set; }
    }
}
