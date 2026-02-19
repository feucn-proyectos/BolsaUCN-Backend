namespace backend.src.Domain.Options
{
    /// <summary>
    /// Opciones de configuración para la gestión de postulaciones a ofertas laborales.
    /// </summary>
    public class JobApplicationOptions
    {
        /// <summary>
        /// Rastrear cambios en la entidad consultada
        /// </summary>
        public bool TrackChanges { get; set; } = false;

        /// <summary>
        /// Incluir información del estudiante que realizó la postulación en la consulta
        /// </summary>
        /// </summary>
        public bool IncludeStudent { get; set; } = false;

        /// <summary>
        /// Incluir información de la oferta laboral a la que se postuló en la consulta
        /// </summary>
        public bool IncludeJobOffer { get; set; } = false;

        /// <summary>
        /// Incluir información de la revisión asociada a la postulación en la consulta, si es que llegara a existir
        /// </summary>
        public bool IncludeReview { get; set; } = false;
    }
}
