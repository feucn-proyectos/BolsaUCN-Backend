namespace backend.src.Application.DTOs.UserDTOs
{
    public class UserImagesDTO
    {
        public IFormFile? ProfilePhoto { get; set; }
        public IFormFile? ProfileBanner { get; set; }
    }
}
