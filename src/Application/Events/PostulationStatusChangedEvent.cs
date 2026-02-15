using System.ComponentModel.DataAnnotations;
using backend.src.Domain.Models;

namespace backend.src.Application.Events
{
    /// <summary>
    /// Event payload used when a postulation (application) status changes.
    /// This DTO is consumed by notification and email services.
    /// </summary>
    public class PostulationStatusChangedEvent
    {
        /// <summary>
        /// Identifier of the postulation that changed status.
        /// </summary>
        public int PostulationId { get; set; }

        /// <summary>
        /// New application status.
        /// </summary>
        [Required(ErrorMessage = "El estado es requerido")]
        [EnumDataType(
            typeof(ApplicationStatus),
            ErrorMessage = "El estado debe ser: Pendiente, Aceptada o Rechazada"
        )]
        public ApplicationStatus NewStatus { get; set; }

        /// <summary>
        /// Title of the offer related to the postulation.
        /// </summary>
        [Required(ErrorMessage = "El nombre de la oferta es requerido")]
        public required string OfferName { get; set; }

        /// <summary>
        /// Name of the company that owns the offer.
        /// </summary>
        [Required(ErrorMessage = "El nombre de la compañía es requerido")]
        public required string CompanyName { get; set; }

        /// <summary>
        /// Student email address that will receive the notification.
        /// </summary>
        [Required(ErrorMessage = "El email del estudiante es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public required string StudentEmail { get; set; }
    }
}
