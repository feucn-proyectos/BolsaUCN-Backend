using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs
{
    public class PublicationsForAdminSearchParamsDTO
    {
        [StringLength(
            100,
            ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres."
        )]
        public string? SearchTerm { get; set; }

        [RegularExpression(
            "Oferta|CompraVenta|Todos",
            ErrorMessage = "FilterByType debe ser 'Oferta', 'CompraVenta' o 'Todos'."
        )]
        public string? FilterByType { get; set; }

        [RegularExpression(
            "Pendiente|Aprobada|Rechazada|Cerrada|Todos",
            ErrorMessage = "FilterByApprovalStatus debe ser 'Pendiente', 'Aprobada', 'Rechazada', 'Cerrada' o 'Todos'."
        )]
        public string? FilterByApprovalStatus { get; set; }

        [RegularExpression(
            "Title|CreatedAt",
            ErrorMessage = "SortBy debe ser 'Title' o 'CreatedAt'."
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
