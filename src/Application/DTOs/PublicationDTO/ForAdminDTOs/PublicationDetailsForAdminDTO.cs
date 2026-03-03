namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs
{
    public class PublicationDetailsForAdminDTO
    {
        public required int PublicationId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime PublicationDate { get; set; }
        public required string PublicationType { get; set; }
        public required string ApprovalStatus { get; set; }
        public required string Location { get; set; }
        public string? AdditionalContactEmail { get; set; }
        public string? AdditionalContactPhoneNumber { get; set; }

        // === INFORMACION DEL AUTOR ===
        public required int UserId { get; set; }
        public required string UserEmail { get; set; }
        public required string UserPhoneNumber { get; set; }
        public required string ProfilePhotoUrl { get; set; }
        public required string UserName { get; set; }
        public required string UserType { get; set; }
        public required string AboutMe { get; set; }
        public required double Rating { get; set; }

        // === ATRIBUTOS DE OFERTA ===
        public string? EndDate { get; set; }
        public string? DeadlineDate { get; set; }
        public int? Remuneration { get; set; }
        public string? OfferType { get; set; }
        public int? ApplicantsCount { get; set; }
        public bool? IsCvRequired { get; set; }
        public string? CurrentStatus { get; set; }

        // === ATRIBUTOS DE COMPRA/VENTA ===
        public string[]? ImageUrls { get; set; }
        public int? Price { get; set; }
        public string? Category { get; set; }
        public int? Quantity { get; set; }
        public string? Availability { get; set; }
        public string? Condition { get; set; }
        public bool? ShowEmail { get; set; }
        public bool? ShowPhoneNumber { get; set; }
    }
}
