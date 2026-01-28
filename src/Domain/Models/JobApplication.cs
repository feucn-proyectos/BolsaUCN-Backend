namespace backend.src.Domain.Models
{
    /// <summary>
    /// Estado de la postulación (Pendiente, Aceptada, Rechazada).
    /// </summary>
    public enum ApplicationStatus
    {
        Pendiente,
        Aceptada,
        Rechazada,
    }

    /// <summary>
    /// Representa una postulación realizada por un estudiante a una oferta laboral.
    /// </summary>
    public class JobApplication : ModelBase
    {
        /// <summary>
        /// El usuario estudiante que realizó la postulación.
        /// </summary>
        public User? Student { get; set; }

        /// <summary>
        /// Identificador del usuario estudiante que realizó la postulación.
        /// </summary>
        public required int StudentId { get; set; }

        /// <summary>
        /// La oferta laboral a la que el estudiante postuló.
        /// </summary>
        public Offer? JobOffer { get; set; }

        /// <summary>
        /// Identificador de la oferta laboral.
        /// </summary>
        public required int JobOfferId { get; set; }

        /// <summary>
        /// Carta de presentación opcional adjuntada por el estudiante.
        /// </summary>
        public string? CoverLetter { get; set; }

        /// <summary>
        /// Estado actual de la postulación (Pendiente, Aceptada, Rechazada).
        /// </summary>
        public required ApplicationStatus Status { get; set; }
    }
}
