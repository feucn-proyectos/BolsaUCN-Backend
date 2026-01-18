using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.DTOs.UserDTOs.UserProfileDTOs
{
    public class GetIndividualProfileDTO : IGetUserProfileDTO
    {
        public required string UserName { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Rut { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required float Rating { get; set; }
        public required string AboutMe { get; set; }
        public string? ProfilePhoto { get; set; }
    }
}
