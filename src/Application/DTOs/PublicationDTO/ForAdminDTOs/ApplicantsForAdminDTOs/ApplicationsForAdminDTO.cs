namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.ApplicantsForAdminDTOs
{
    public class ApplicationsForAdminDTO
    {
        public required List<ApplicationForAdminDTO> Applications { get; set; }
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
        public required int PageSize { get; set; }
        public required int CurrentPage { get; set; }
    }
}
