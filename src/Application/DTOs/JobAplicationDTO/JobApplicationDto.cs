namespace backend.src.Application.DTOs.JobAplicationDTO
{
    public class CreateJobApplicationDto
    {
        public int JobOfferId { get; set; }
        public string? MotivationLetter { get; set; }
    }

    public class JobApplicationResponseDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string OfferTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public string? CurriculumVitae { get; set; }
        public string? MotivationLetter { get; set; }
    }

    public class JobApplicationDetailDto
    {
        // Encabezado
        public string OfferTitle { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        // Información de fechas y remuneración
        public DateTime ApplicationDate { get; set; } // Postulada hasta: 20/09/2025

        public DateTime PublicationDate { get; set; } // Fecha de publicación: 01/09/2025
        public DateTime? EndDate { get; set; } // Duración fin
        public int Remuneration { get; set; } // Remuneración: $5.000 CLP

        // Descripción y requisitos
        public string? Description { get; set; } // Descripción completa
        public string? Requirements { get; set; } // Requisitos

        // Contacto
        public string? ContactInfo { get; set; } // Teléfono o email de contacto

        // Estado de la postulación
        public int Id { get; set; } // ID de la postulación
        public string Status { get; set; } = string.Empty; // Pendiente/Aceptado/Rechazado
        public string? StatusMessage { get; set; } // Mensaje según estado
    }
}
