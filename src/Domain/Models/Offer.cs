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

    public enum ReviewStatus
    {
        NoDisponible, // Para publicaciones que no son ofertas o que no están listas para revisión
        SinRevisar, // Estado inicial, sin revisiones realizadas
        RevisadaPorOferta, // Revisada por el oferente (solo para ofertas)
        RevisadaPorPostulante, // Revisada por el postulante (solo para ofertas)
        RevisadaPorAmbos, // Revisada tanto por oferente como por postulante (solo para ofertas)
    }

    /// <summary>
    /// Representa una oferta de trabajo o voluntariado publicada en el sistema.
    /// Hereda propiedades comunes de publicación de <see cref="Publication"/>.
    /// </summary>
    public class Offer : Publication
    {
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
        public int AvailableSlots { get; set; }

        /// <summary>
        /// Categoría de la oferta (e.g., Trabajo, Voluntariado).
        /// </summary>
        public required OfferTypes OfferType { get; set; }

        /// <summary>
        /// Colección de postulaciones realizadas a esta oferta.
        /// </summary>
        public ICollection<JobApplication> Applications { get; set; } = [];

        /// <summary>
        /// Indica el estado de revision de la oferta.
        /// </summary>
        public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.NoDisponible;

        /// <summary>
        /// Indica si es obligatorio subir un CV para postular.
        /// true = CV obligatorio; false = CV opcional.
        /// Por defecto es true.
        /// </summary>
        public bool IsCvRequired { get; set; } = true;
    }
}
