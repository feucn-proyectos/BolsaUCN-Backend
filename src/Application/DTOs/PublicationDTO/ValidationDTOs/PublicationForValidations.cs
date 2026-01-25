using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.PublicationDTO.ValidationDTOs
{
    public class PublicationForValidationDTO
    {
        public required int PublicationId { get; set; }
        public required string Title { get; set; }
        public required string Type { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string CreatedBy { get; set; }
        public required StatusValidation Status { get; set; }
        public string? OfferType { get; set; }
        public decimal? Price { get; set; }
    }
}
