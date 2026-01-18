using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.JobAplicationDTO
{
    public class ViewApplicantUserDetailDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public required float? Rating { get; set; }
        public required string MotivationLetter { get; set; }
        public required string Disability { get; set; }
        public required string CurriculumVitae { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
