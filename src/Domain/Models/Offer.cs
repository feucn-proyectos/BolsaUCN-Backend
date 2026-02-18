using Hangfire.Common;

namespace backend.src.Domain.Models
{
    /// <summary>
    /// Tipos de oferta (trabajo o voluntariado).
    /// </summary>
    public enum OfferTypes
    {
        Trabajo,
        Voluntariado,
    }

    public enum OfferStatus
    {
        EnRevision, // Oferta creada y en revisión administrativa, aún no visible para usuarios regulares
        RecibiendoPostulaciones, // Oferta aprobada y visible para usuarios regulares, recibiendo postulaciones activamente
        RealizandoTrabajo, // Oferta cerrada para nuevas postulaciones, en proceso de realización del trabajo o voluntariado
        CalificacionesEnProceso, // Trabajo o voluntariado finalizado, en proceso de calificaciones y reseñas por parte de oferente y postulantes
        Finalizada, // Oferta completamente finalizada, con calificaciones y reseñas completadas, visible en el historial de ofertas del usuario
        CanceladaAntesDelTrabajo, // Oferta cancelada por el oferente antes de que se cierre para postulaciones, no visible para usuarios regulares
    }

    /// <summary>
    /// Representa una oferta de trabajo o voluntariado publicada en el sistema.
    /// Hereda propiedades comunes de publicación de <see cref="Publication"/>.
    /// </summary>
    public class Offer : Publication
    {
        /// <summary>
        /// Estado actual de la oferta en su ciclo de vida.
        /// </summary>
        public required OfferStatus OfferStatus { get; set; } = OfferStatus.EnRevision;

        /// <summary>
        /// Fecha de finalización de la oferta (cuando termina la posición o oportunidad).
        /// </summary>
        public required DateTime EndDate { get; set; }

        /// <summary>
        /// Fecha límite de postulación para la oferta.
        /// </summary>
        public required DateTime ApplicationDeadline { get; set; }

        /// <summary>
        /// Remuneración ofrecida en pesos chilenos. No aplica para posiciones voluntarias.
        /// </summary>
        public int? Remuneration { get; set; }

        /// <summary>
        /// Número de postulantes requeridos para la oferta.
        /// </summary>
        public int AvailableSlots { get; set; } = 1;

        /// <summary>
        /// Categoría de la oferta (e.g., Trabajo, Voluntariado).
        /// </summary>
        public required OfferTypes OfferType { get; set; }

        /// <summary>
        /// Colección de postulaciones realizadas a esta oferta.
        /// </summary>
        public ICollection<JobApplication> Applications { get; set; } = [];

        /// <summary>
        /// Indica si es obligatorio subir un CV para postular.
        /// true = CV obligatorio; false = CV opcional.
        /// Por defecto es true.
        /// </summary>
        public bool IsCvRequired { get; set; } = true;
    }
}
