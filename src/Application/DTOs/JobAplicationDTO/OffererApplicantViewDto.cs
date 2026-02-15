// En: src/Application/DTOs/JobAplicationDTO/OffererApplicantViewDto.cs
// (Reemplaza el contenido por este)
using System;

namespace backend.src.Application.DTOs.JobAplicationDTO
{
    public class OffererApplicantViewDto
    {
        public int ApplicationId { get; set; }
        public int StudentId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public string? CurriculumVitaeUrl { get; set; }
        public double Rating { get; set; }
    }
}
