using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs
{
    public class MyPublicationsSeachParamsDTO
    {
        [MaxLength(
            100,
            ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres."
        )]
        public string? SearchTerm { get; set; }

        [RegularExpression("Oferta|CompraVenta", ErrorMessage = "Tipo de usuario inválido.")]
        public string? FilterByPublicationType { get; set; }

        [RegularExpression(
            "Aprobada|Rechazada|Pendiente",
            ErrorMessage = "Estado de aprobación inválido."
        )]
        public string? FilterByApprovalStatus { get; set; }

        [RegularExpression("Title|CreatedAt", ErrorMessage = "Campo de ordenamiento inválido.")]
        public string? SortBy { get; set; }

        [RegularExpression("asc|desc", ErrorMessage = "Orden de ordenamiento inválido.")]
        public string? SortOrder { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Numero de pagina debe ser al menos 1.")]
        public required int PageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "Tamaño de página debe estar entre 1 y 100.")]
        public int? PageSize { get; set; }
    }
}
