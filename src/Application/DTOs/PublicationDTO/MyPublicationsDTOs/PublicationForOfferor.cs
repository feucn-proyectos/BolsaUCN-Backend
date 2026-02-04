namespace backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs
{
    public class PublicationForOfferor
    {
        public required int IdPublication { get; set; }
        public required string Title { get; set; }
        public required string PublicationType { get; set; }
        public required DateTime PublicationDate { get; set; }
        public required string ApprovalStatus { get; set; }
        public required bool IsOpen { get; set; }
        public required bool HasAppealed { get; set; }
    }
}
