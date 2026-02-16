namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.ApplicantsForAdminDTOs
{
    public class ApplicationForAdminDTO
    {
        public required int ApplicationId { get; set; }
        public required int ApplicantId { get; set; }
        public required string ApplicantPhotoUrl { get; set; }
        public required string ApplicantFirstName { get; set; }
        public required string ApplicantLastName { get; set; }
        public required string ApplicantEmail { get; set; }
        public required string ApplicationDate { get; set; }
        public required string Status { get; set; } // Pendiente, Aceptada, Rechazada
        public required bool HasCV { get; set; }
        public string? CoverLetter { get; set; }
    }
}
