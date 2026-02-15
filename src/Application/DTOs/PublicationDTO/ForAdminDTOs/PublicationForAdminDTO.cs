namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs
{
    public class PublicationForAdminDTO
    {
        public required int Id { get; set; }
        public required string PublicationType { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Location { get; set; }
        public required string ApprovalStatus { get; set; }
        public required int AppealsCount { get; set; }
        public required DateTime CreatedAt { get; set; }

        // Informacion del autor
        public required int AuthorId { get; set; }
        public required string AuthorName { get; set; }
        public required string UserType { get; set; }
        public required string ProfilePhotoUrl { get; set; }
        public required string AuthorEmail { get; set; }
    }
}
