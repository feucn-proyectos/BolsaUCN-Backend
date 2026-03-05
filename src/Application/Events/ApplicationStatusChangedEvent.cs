using System.ComponentModel.DataAnnotations;
using backend.src.Application.Events.Implements;
using backend.src.Domain.Models;

namespace backend.src.Application.Events
{
    /// <summary>
    /// Evento que se dispara cuando el estado de una postulación cambia, contiene información relevante para notificar al estudiante.
    /// </summary>
    public class ApplicationStatusChangedEvent : DomainEvent
    {
        /// <summary>
        /// ID de la postulación cuyo estado ha cambiado.
        /// </summary>
        public int ApplicationId { get; set; }

        /// <summary>
        /// Nuevo estado de la postulación después del cambio.
        /// </summary>
        [Required(ErrorMessage = "El estado es requerido")]
        [EnumDataType(
            typeof(ApplicationStatus),
            ErrorMessage = "El estado debe ser: Pendiente, Aceptada o Rechazada"
        )]
        public ApplicationStatus NewStatus { get; set; }

        /// <summary>
        /// Nombre de la oferta relacionada con la postulación.
        /// </summary>
        [Required(ErrorMessage = "El nombre de la oferta es requerido")]
        public required string OfferName { get; set; }

        /// <summary>
        /// Nombre del oferente relacionado con la oferta.
        /// </summary>
        [Required(ErrorMessage = "El nombre de la compañía es requerido")]
        public required string OfferorName { get; set; }

        /// <summary>
        /// Nombre del estudiante que realizó la postulación.
        /// </summary>
        [Required(ErrorMessage = "El nombre del estudiante es requerido")]
        public required string ApplicantName { get; set; }

        /// <summary>
        /// Email del estudiante que realizó la postulación, utilizado para enviar notificaciones.
        /// </summary>
        [Required(ErrorMessage = "El email del estudiante es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public required string ApplicantEmail { get; set; }
    }
}
