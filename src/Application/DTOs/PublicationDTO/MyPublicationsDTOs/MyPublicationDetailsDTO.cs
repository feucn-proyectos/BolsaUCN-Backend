namespace backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs
{
    /// <summary>
    /// DTO que representa los detalles de una publicación específica del usuario.
    /// </summary>
    public class MyPublicationDetailsDTO
    {
        // === PROPIEDADES COMUNES A TODAS LAS PUBLICACIONES ===
        // Informacion basica
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Location { get; set; }

        // Informacion de contacto
        public required string ContactEmail { get; set; }
        public required string ContactPhone { get; set; }
        public string? AdditionalContactEmail { get; set; }
        public string? AdditionalContactPhoneNumber { get; set; }

        // Metadata
        public required string PublicationType { get; set; }
        public required string ApprovalStatus { get; set; }
        public required DateTime CreatedAt { get; set; }

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
