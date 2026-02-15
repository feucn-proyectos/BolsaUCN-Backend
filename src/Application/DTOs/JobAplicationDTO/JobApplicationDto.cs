namespace backend.src.Application.DTOs.JobAplicationDTO
{
    public class CreateJobApplicationDto
    {
        public int JobOfferId { get; set; }
        public string? MotivationLetter { get; set; }
    }

    public class JobApplicationResponseDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string OfferTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public string? CurriculumVitae { get; set; }
        public string? MotivationLetter { get; set; }
    }
}
