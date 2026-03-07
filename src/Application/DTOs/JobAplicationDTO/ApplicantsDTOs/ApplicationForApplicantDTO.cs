namespace backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs
{
    public class ApplicationForApplicantDTO
    {
        public required int Id { get; set; }
        public required string OfferTitle { get; set; }
        public required string Status { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
