namespace backend.src.Application.DTOs.PublicationDTO.ValidationDTOs
{
    /// <summary>
    /// DTO para representar la acción de validación en una publicación.
    /// </summary>
    public class PublicationDetailsForApprovalDTO
    {
        public required int PublicationId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime PublicationDate { get; set; }
        public required string PublicationType { get; set; }
        public required string ApprovalStatus { get; set; }
        public required string Location { get; set; }
        public required int NumberOfAppeals { get; set; }
        public required string? LastRejectionReason { get; set; }
        public string? AdditionalContactEmail { get; set; }
        public string? AdditionalContactPhoneNumber { get; set; }

        // === INFORMACION DEL AUTOR ===
        public required int UserId { get; set; }
        public required string UserEmail { get; set; }
        public required string UserName { get; set; }
        public required string ProfilePhotoUrl { get; set; }
        public required string AboutMe { get; set; }
        public required float Rating { get; set; }

        // === ATRIBUTOS DE OFERTA ===
        public string? EndDate { get; set; }
        public string? ApplicationDeadline { get; set; }
        public int? Remuneration { get; set; }
        public bool? IsCVRequired { get; set; }
        public string? OfferType { get; set; }

        // === ATRIBUTOS DE COMPRA/VENTA ===
        public bool? IsEmailAvailable { get; set; }
        public bool? IsPhoneNumberAvailable { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? Category { get; set; }
        public int? Price { get; set; }
        public int? Quantity { get; set; }
        public string? Condition { get; set; }
    }
}
