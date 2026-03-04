namespace backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs
{
    public class PublicationForOfferorDTO
    {
        public required int PublicationId { get; set; }
        public required string Title { get; set; }
        public required string PublicationType { get; set; }
        public required DateTime PublicationDate { get; set; }
        public required string ApprovalStatus { get; set; }
        public required bool HasAppealed { get; set; }
        public string? Availability { get; set; }
    }
}
