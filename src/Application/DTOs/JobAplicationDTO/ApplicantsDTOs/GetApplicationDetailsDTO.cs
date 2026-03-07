namespace backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs
{
    public class GetApplicationDetailsDTO
    {
        // Oferta
        public required string OfferTitle { get; set; }
        public required string Description { get; set; }
        public required DateTime ApplicationDeadline { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime EndDate { get; set; }
        public required int Remuneration { get; set; }

        // Oferente
        public required string OfferorName { get; set; }
        public required string OfferorUserType { get; set; }
        public required string ProfilePhotoUrl { get; set; }

        // Contacto
        public required string ContactEmail { get; set; }
        public required string ContactPhoneNumber { get; set; }
        public string? AdditionalContactEmail { get; set; }
        public string? AdditionalContactPhoneNumber { get; set; }

        // Datos de la postulación
        public required int Id { get; set; }
        public string? CoverLetter { get; set; }
        public required string Status { get; set; }
        public required string StatusMessage { get; set; }
    }
}
