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
        EnRevision,
        RecibiendoPostulaciones,
        RealizandoTrabajo,
        CalificacionesEnProceso,
        Finalizada,
        CanceladaAntesDelTrabajo,
    }

    /// <summary>
    /// Representa una oferta de trabajo o voluntariado publicada en el sistema.
    /// Hereda propiedades comunes de publicación de <see cref="Publication"/>.
    /// </summary>
    public class Offer : Publication
    {
        /// <summary>
        /// Fecha en la que se termina el trabajo.
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

        // === Atributos para el seguimiento ===

        public string? CloseApplicationsJobId { get; set; }
        public string? FinishWorkAndInitializeReviewsJobId { get; set; }
        public string? FinalizeAndCloseReviewsJobId { get; set; }
        public DateTime? WorkStartedAt { get; set; }
        public DateTime? WorkCompletedAt { get; set; }

        /// <summary>
        /// Fecha en la que la oferta llega al final de su ciclo de vida.
        /// A lo mas es 2 semanas despues de la fecha de termino.
        /// </summary>
        public DateTime? FinalizedAt { get; set; }

        /// <summary>
        /// Fecha en que la oferta es cancelada por el oferente antes de que se cierre para postulaciones.
        /// Una vez que han terminado las postulaciones no te puedes arrepentir.
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        // === Atributos derivados ===
        public bool IsInAdminReview => ApprovalStatus == ApprovalStatus.Pendiente;
        public bool IsRejected => ApprovalStatus == ApprovalStatus.Rechazada;
        public bool IsAcceptingApplications =>
            ApprovalStatus == ApprovalStatus.Aceptada
            && DateTime.UtcNow <= ApplicationDeadline
            && WorkStartedAt == null;
        public bool IsWorkInProgress =>
            ApprovalStatus == ApprovalStatus.Aceptada
            && WorkStartedAt.HasValue
            && !WorkCompletedAt.HasValue;
        public bool IsAwaitingReviews =>
            ApprovalStatus == ApprovalStatus.Aceptada
            && WorkCompletedAt.HasValue
            && !FinalizedAt.HasValue;
        public bool IsFinalized =>
            ApprovalStatus == ApprovalStatus.Aceptada && FinalizedAt.HasValue; // Se finaliza automáticamente 2 semanas después de la fecha de término si no se ha finalizado manualmente antes
        public bool IsCancelled => CancelledAt.HasValue;

        // === Metodos para el cambio de estado ===
        /// <summary>
        /// Marca el inicio del trabajo para esta oferta, estableciendo la fecha de inicio y permitiendo avanzar al estado de "Realizando Trabajo".
        /// </summary>
        public void StartWork()
        {
            if (ApprovalStatus != ApprovalStatus.Aceptada)
                throw new InvalidOperationException("Oferta no aprobada");
            if (WorkStartedAt.HasValue)
                throw new InvalidOperationException("Trabajo ya iniciado");

            WorkStartedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marca la oferta como trabajo completado, estableciendo la fecha de finalización del trabajo y permitiendo avanzar al estado de "Calificaciones en Proceso".
        /// </summary>
        public void CompleteWork()
        {
            if (!WorkStartedAt.HasValue)
                throw new InvalidOperationException("Trabajo no iniciado");
            if (WorkCompletedAt.HasValue)
                throw new InvalidOperationException("Trabajo ya completado");

            WorkCompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marca la oferta como finalizada, estableciendo la fecha de finalización y permitiendo avanzar al estado de "Finalizada".
        /// </summary>
        public void FinalizeOffer()
        {
            if (!WorkCompletedAt.HasValue)
                throw new InvalidOperationException("Trabajo no completado");
            if (FinalizedAt.HasValue)
                throw new InvalidOperationException("Ya finalizado");

            FinalizedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Cancela la oferta antes de que se cierre para postulaciones, estableciendo la fecha de cancelación y permitiendo avanzar al estado de "CanceladaAntesDelTrabajo".
        /// </summary>
        public void CancelOffer()
        {
            if (WorkStartedAt.HasValue)
                throw new InvalidOperationException(
                    "No se puede cancelar una oferta con trabajo iniciado"
                );
            if (CancelledAt.HasValue)
                throw new InvalidOperationException("Oferta ya cancelada");

            CancelledAt = DateTime.UtcNow;
        }

        // === Estado para el frontend ===
        /// <summary>
        /// Estado actual de la oferta, derivado de sus propiedades y fechas. Este atributo no se almacena en la base de datos, sino que se calcula dinámicamente para facilitar la lógica del frontend.
        /// </summary>
        public OfferStatus CurrentStatus
        {
            get
            {
                if (IsCancelled)
                    return OfferStatus.CanceladaAntesDelTrabajo;
                if (IsFinalized)
                    return OfferStatus.Finalizada;
                if (IsAwaitingReviews)
                    return OfferStatus.CalificacionesEnProceso;
                if (IsWorkInProgress)
                    return OfferStatus.RealizandoTrabajo;
                if (IsAcceptingApplications)
                    return OfferStatus.RecibiendoPostulaciones;
                if (IsInAdminReview)
                    return OfferStatus.EnRevision;

                // Si no cumple ninguna condición, se asume que está en revisión por defecto
                return OfferStatus.EnRevision;
            }
        }
    }
}
