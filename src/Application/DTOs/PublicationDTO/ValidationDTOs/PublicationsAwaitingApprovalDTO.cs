namespace backend.src.Application.DTOs.PublicationDTO.ValidationDTOs
{
    public class PublicationsAwaitingApprovalDTO
    {
        public List<PublicationAwaitingApprovalDTO> Publications { get; set; } = [];
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
