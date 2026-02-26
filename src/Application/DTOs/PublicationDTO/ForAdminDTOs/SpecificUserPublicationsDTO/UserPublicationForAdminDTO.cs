namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.SpecificUserPublicationsDTO
{
    public class UserPublicationForAdminDTO
    {
        public int PublicationId { get; set; }
        public required string Title { get; set; }
        public required string PublicationStatus { get; set; }
        public required string PublicationType { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required bool HasBeenAppealed { get; set; }
    }
}
