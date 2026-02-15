namespace backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs
{
    public class ApplicationsForApplicantDTO
    {
        public required List<ApplicationForApplicantDTO> Applications { get; set; } = [];
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
        public required int CurrentPage { get; set; }
        public required int PageSize { get; set; }
    }
}
