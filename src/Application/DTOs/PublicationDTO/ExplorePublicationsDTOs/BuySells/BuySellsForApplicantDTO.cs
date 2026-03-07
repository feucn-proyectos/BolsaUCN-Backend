namespace backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.BuySells
{
    public class BuySellsForApplicantDTO
    {
        public required List<BuySellForApplicantDTO> BuySells { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
