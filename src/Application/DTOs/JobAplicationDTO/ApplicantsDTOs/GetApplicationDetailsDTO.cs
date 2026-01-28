namespace backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs
{
    public class GetApplicationDetailsDTO
    {
        // Encabezado
        public required string OfferTitle { get; set; }
        public required string CompanyName { get; set; }

        // Información de fechas y remuneración
        public DateTime ApplicationDeadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EndDate { get; set; }
        public int Remuneration { get; set; }

        // Descripción y requisitos
        public string? Description { get; set; }
        public string? Requirements { get; set; }

        // Contacto
        public string? ContactInfo { get; set; }

        // Datos de la postulación
        public required int Id { get; set; }
        public string? CoverLetter { get; set; }
        public required string Status { get; set; }
        public string? StatusMessage { get; set; }
    }
}
