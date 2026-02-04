namespace backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs
{
    public class ApplicationDetailsDTO
    {
        public required int ApplicationId { get; set; }
        public required int ApplicantId { get; set; }
        public required string ApplicantFirstName { get; set; }
        public required string ApplicantLastName { get; set; }
        public required string ApplicantEmail { get; set; }
        public required DateTime ApplicationDate { get; set; }
        public string? CVUrl { get; set; }
        public string? CoverLetter { get; set; }
    }
}
