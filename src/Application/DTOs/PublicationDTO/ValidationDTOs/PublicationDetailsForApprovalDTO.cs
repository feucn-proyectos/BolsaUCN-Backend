namespace backend.src.Application.DTOs.PublicationDTO.ValidationDTOs
{
    /// <summary>
    /// DTO para representar la acción de validación en una publicación.
    /// </summary>
    public class PublicationDetailsForApprovalDTO
    {
        public required int PublicationId { get; set; }
        public required int UserId { get; set; }
        public required string UserEmail { get; set; }
        public required string UserName { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required List<string> Images { get; set; }
        public required DateTime PublicationDate { get; set; }
        public required string PublicationType { get; set; }
        public required bool IsOpen { get; set; }
        public required string ApprovalStatus { get; set; }
        public required string Location { get; set; }
        public string? AdditionalContactEmail { get; set; }
        public string? AdditionalContactPhoneNumber { get; set; }
        public required string AboutMe { get; set; }
        public required double Rating { get; set; }

        // === ATRIBUTOS DE OFERTA ===
        public string? EndDate { get; set; }
        public string? DeadlineDate { get; set; }
        public string? Requirements { get; set; }
        public int? Remuneration { get; set; }
        public string? OfferType { get; set; }

        // === ATRIBUTOS DE COMPRA/VENTA ===
        public string? Category { get; set; }
        public int? Price { get; set; }

        // === LEGACY ATTRIBUTES FOR FRONTEND ===
        public required bool Active { get; set; }
        public required bool IsActive { get; set; }
        public required List<string> ImageUrls { get; set; }
        public required string CompanyName { get; set; }
    }
}
