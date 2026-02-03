namespace backend.src.Application.DTOs.UserDTOs.UserProfileDTOs
{
    public class GetUserProfileDTO
    {
        public required string UserName { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Rut { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string AboutMe { get; set; }
        public required double Rating { get; set; }
        public string? CurriculumVitae { get; set; }
        public string? Disability { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? PendingEmail { get; set; }
    }
}
