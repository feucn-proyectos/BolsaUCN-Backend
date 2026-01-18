namespace backend.src.Domain.Models
{
    /// <summary>
    /// Clase base para los modelos de dominio que incluye propiedades comunes.
    /// </summary>
    public class ModelBase
    {
        /// <summary>
        /// Identificador único de la entidad.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha y hora de creación de la entidad (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora de la última actualización de la entidad (UTC).
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
