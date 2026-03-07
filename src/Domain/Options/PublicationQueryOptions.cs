using backend.src.Domain.Models;

namespace backend.src.Domain.Options
{
    /// <summary>
    /// Opciones para consultas de publicaciones
    /// </summary>
    public class PublicationQueryOptions
    {
        /// <summary>
        /// Rastrear cambios en la entidad consultada
        /// </summary>
        public bool TrackChanges { get; set; } = false;

        /// <summary>
        /// Incluir imágenes asociadas a la publicación en la consulta
        /// </summary>
        public bool IncludeImages { get; set; } = false;

        /// <summary>
        /// Incluir aplicaciones asociadas a la publicación en la consulta
        /// </summary>
        public bool IncludeApplications { get; set; } = false;

        /// <summary>
        /// Incluir usuario que creó la publicación en la consulta
        /// </summary>
        public bool IncludeUser { get; set; } = false;
    }
}
