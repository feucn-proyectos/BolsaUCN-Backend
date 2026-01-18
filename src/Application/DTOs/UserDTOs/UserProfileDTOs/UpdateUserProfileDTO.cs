using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.UserDTOs.UserProfileDTOs
{
    /// <summary>
    /// DTO para actualizar el perfil de un usuario.
    /// </summary>
    public class UpdateUserProfileDTO
    {
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? AboutMe { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
