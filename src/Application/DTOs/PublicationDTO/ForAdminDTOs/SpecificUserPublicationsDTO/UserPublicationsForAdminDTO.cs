namespace backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.SpecificUserPublicationsDTO
{
    public class UserPublicationsForAdminDTO
    {
        public List<UserPublicationForAdminDTO> Publications { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
