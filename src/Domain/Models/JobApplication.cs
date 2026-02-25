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
        CanceladaPorPostulante,
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

        /// <summary>
        /// Indica si el postulante ha actualizado o eliminado su CV despues de que la postulacion se haya cerrado.
        /// Sirve como auditoria para la administracion, para que al revisar el historial de postulaciones puedan saber si el CV que se reviso en su momento es el mismo que el actual del postulante o no.
        /// </summary>
        public bool IsCVInvalid { get; set; } = false;
    }
}
