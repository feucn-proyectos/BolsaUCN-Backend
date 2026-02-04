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
