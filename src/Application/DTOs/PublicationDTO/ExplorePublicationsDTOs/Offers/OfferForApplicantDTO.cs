namespace backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers
{
    public class OfferForApplicantDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string AuthorName { get; set; }
        public required string OfferType { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime ApplicationDeadline { get; set; }
        public required int AvailableSlots { get; set; }
        public required int Remuneration { get; set; }
    }
}
