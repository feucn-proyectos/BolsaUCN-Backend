using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO detallado para ver una publicación de compra/venta específica
    /// </summary>
    public class BuySellDetailDTO
    {
        /// <summary>
        /// Identificador único de la publicación
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Título de la publicación
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Descripción de la publicación
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Categoría de la publicación
        /// </summary>
        public string Category { get; set; } = null!;

        /// <summary>
        /// Precio de la publicación
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Ubicación de la publicación
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Información de contacto del propietario de la publicación
        /// </summary>
        public string? ContactInfo { get; set; }

        /// <summary>
        /// Fecha de publicación
        /// </summary>
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Indica si la publicación está activa o no
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// URLs de las imágenes asociadas a la publicación
        /// </summary>
        public List<string> ImageUrls { get; set; } = [];

        /// <summary>
        /// Estado de validación de la publicación
        /// </summary>
        public required StatusValidation StatusValidation { get; set; }

        // Información del usuario

        /// <summary>
        /// Identificador del usuario que creó la publicación
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Nombre del usuario que creó la publicación
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Email del usuario que creó la publicación
        /// </summary>
        public string UserEmail { get; set; } = null!;

        /// <summary>
        /// Información "Sobre mí" del usuario que creó la publicación
        /// </summary>
        public string? AboutMe { get; set; }

        /// <summary>
        /// Calificación promedio del usuario que creó la publicación
        /// </summary>
        public double Rating { get; set; }
    }
}
