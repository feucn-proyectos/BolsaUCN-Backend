namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs
{
    public class PublicationsForAdminDTO
    {
        public List<PublicationForAdminDTO> Publications { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
