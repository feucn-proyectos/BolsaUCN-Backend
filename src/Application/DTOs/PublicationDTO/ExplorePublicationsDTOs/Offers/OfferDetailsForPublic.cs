namespace backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers
{
    public class OfferDetailsForPublicDTO
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
    }
}
