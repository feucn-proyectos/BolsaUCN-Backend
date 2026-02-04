namespace backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs
{
    /// <summary>
    /// DTO que representa los detalles de una publicación específica del usuario.
    /// </summary>
    public class MyPublicationDetailsDTO
    {
        // === PROPIEDADES COMUNES A TODAS LAS PUBLICACIONES ===
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string? AdditionalContactEmail { get; set; }
        public string? AdditionalContactPhone { get; set; }
        public string PublicationType { get; set; } = null!;
        public string ApprovalStatus { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // === OFERTAS DE TRABAJO ===
        public string? OfferType { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public int? Remuneration { get; set; }
        public bool? IsCvRequired { get; set; }
        public int? ApplicationsCount { get; set; }

        // === COMPRA / VENTAS ===
        public string[]? ImageUrls { get; set; }
        public int? Price { get; set; }
        public string? Category { get; set; }
        public int? Quantity { get; set; }
        public string? Availability { get; set; }
        public string? Condition { get; set; }
    }
}
