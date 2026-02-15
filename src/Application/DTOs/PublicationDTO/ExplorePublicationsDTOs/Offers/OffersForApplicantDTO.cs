namespace backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers
{
    public class OffersForApplicantDTO
    {
        public required List<OfferForApplicantDTO> Offers { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
