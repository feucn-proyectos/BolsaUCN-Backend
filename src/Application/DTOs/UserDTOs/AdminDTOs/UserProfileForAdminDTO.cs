using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.UserDTOs.AdminDTOs
{
    public class UserProfileForAdminDTO
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Rut { get; set; }
        public float? Rating { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? AboutMe { get; set; }
        public required string UserType { get; set; }
        public bool Banned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Student specific fields
        public string? CVUrl { get; set; }
        public string? Disability { get; set; }

        // Admin specific fields
        public bool? SuperAdmin { get; set; }
    }
}
