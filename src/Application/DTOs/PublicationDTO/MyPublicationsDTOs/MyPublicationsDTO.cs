namespace backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs
{
    public class MyPublicationsDTO
    {
        public required List<PublicationForOfferorDTO> Publications { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
