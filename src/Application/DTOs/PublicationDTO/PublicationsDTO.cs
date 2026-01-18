using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO que presenta una publicacion
    /// </summary>
    public class PublicationsDTO
    {
        /// <summary>
        /// Identificador único de la publicación
        /// </summary>
        public int IdPublication { get; set; }

        /// <summary>
        /// Identificador del usuario que creó la publicación
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Título de la publicación
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de publicación (Oferta o CompraVenta)
        /// </summary>
        public Types Types { get; set; }

        /// <summary>
        /// Descripción de la publicación
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de publicación
        /// </summary>
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Imágenes asociadas a la publicación
        /// </summary>
        public ICollection<Image> Images { get; set; } = new List<Image>();

        /// <summary>
        /// Indica si la publicación está activa o no
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Estado de validación de la publicación
        /// </summary>
        public StatusValidation StatusValidation { get; set; }
    }
}
