namespace backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers
{
    public class OfferDetailsForApplicantDTO
    {
        public required int Id { get; set; }
        public required string OfferType { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string AuthorName { get; set; }
        public required string Location { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required int Remuneration { get; set; }
        public required bool IsCVRequired { get; set; }
        public required DateTime EndDate { get; set; }
        public required DateTime ApplicationDeadline { get; set; }
        public required bool HasApplied { get; set; }
        public required int AvailableSlots { get; set; }
        public required string ContactEmail { get; set; }
        public required string ContactPhoneNumber { get; set; }
        public string? AdditionalContactEmail { get; set; }
        public string? AdditionalContactPhoneNumber { get; set; }
    }
}
