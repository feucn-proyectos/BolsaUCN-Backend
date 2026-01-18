using backend.src.Application.DTOs.UserDTOs.UserProfileDTOs;

namespace backend.src.Application.DTOs.UserDTOs.AdminDTOs
{
    public class UsersForAdminDTO
    {
        public List<UserForAdminDTO> Users { get; set; } = new List<UserForAdminDTO>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
