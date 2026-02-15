namespace backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs
{
    public class ApplicationsForOfferorDTO
    {
        public required List<ApplicationForOfferorDTO> Applications { get; set; }
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
        public required int PageSize { get; set; }
        public required int CurrentPage { get; set; }
    }
}
