namespace backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs
{
    public class BuySellForApplicantDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string AuthorName { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string Category { get; set; }
        public required decimal Price { get; set; }
    }
}
