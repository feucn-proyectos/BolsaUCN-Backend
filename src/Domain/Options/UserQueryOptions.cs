namespace backend.src.Domain.Models.Options
{
    /// <summary>
    /// Opciones para consultas de usuarios
    /// </summary>
    public class UserQueryOptions
    {
        /// <summary>
        /// Incluir foto del usuario en la consulta
        /// </summary>
        public bool IncludePhoto { get; set; } = false;

        /// <summary>
        /// Incluir CV del usuario en la consulta
        /// </summary>
        public bool IncludeCV { get; set; } = false;

        /// <summary>
        /// Incluir aplicaciones del usuario en la consulta
        /// </summary>
        public bool IncludeApplications { get; set; } = false;

        /// <summary>
        /// Incluir publicaciones del usuario en la consulta
        /// </summary>
        public bool IncludePublications { get; set; } = false;

        /// <summary>
        /// Rastrear cambios en la entidad consultada
        /// </summary>
        public bool TrackChanges { get; set; } = false;
    }
}
