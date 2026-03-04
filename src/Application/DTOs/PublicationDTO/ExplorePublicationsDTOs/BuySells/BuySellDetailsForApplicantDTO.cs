namespace backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.BuySells
{
    public class BuySellDetailsForApplicantDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string AuthorName { get; set; }
        public required bool IsAvailable { get; set; }
        public required string Location { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required int Price { get; set; }
        public required string Category { get; set; }
        public required int Quantity { get; set; }
        public required string Condition { get; set; }
        public required string ChosenContactEmail { get; set; }
        public required string ChosenContactPhoneNumber { get; set; }
        public required List<string> ImageUrls { get; set; }
    }
}
