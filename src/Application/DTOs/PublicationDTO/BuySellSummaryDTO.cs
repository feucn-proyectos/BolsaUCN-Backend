namespace backend.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO resumido para listar publicaciones de compra/venta
    /// </summary>
    public class BuySellSummaryDTO
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
        /// Fecha de publicación
        /// </summary>
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// URL de la primera imagen de la publicación (si existe)
        /// </summary>
        public string? FirstImageUrl { get; set; }

        // Información del usuario

        /// <summary>
        /// Identificador del usuario que creó la publicación
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        ///  Nombre del usuario que creó la publicación
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Calificación promedio del usuario que creó la publicación
        /// </summary>
        public double UserRating { get; set; }
    }
}
