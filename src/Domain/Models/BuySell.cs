namespace backend.src.Domain.Models
{
    /// <summary>
    /// Disponibilidad del producto (disponible o vendido).
    /// </summary>
    public enum Availability
    {
        Disponible,
        Vendido,
    }

    /// <summary>
    /// Estado del producto (nuevo o usado).
    /// </summary>
    public enum Condition
    {
        Nuevo,
        ComoNuevo,
        Usado,
        NoAplica,
    }

    /// <summary>
    /// Representa una publicación de compra/venta en el sistema.
    /// Hereda propiedades comunes de publicación de <see cref="Publication"/>.
    /// </summary>
    public class BuySell : Publication
    {
        /// <summary>
        /// Coleccion de imágenes asociadas a la publicación.
        /// </summary>
        public ICollection<Image> Images { get; set; } = [];

        /// <summary>
        /// Precio del producto o servicio en pesos chilenos.
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Categoría del producto o servicio.
        /// </summary>
        public required string Category { get; set; }

        /// <summary>
        /// Cantidad disponible del producto.
        /// </summary>
        public required int Quantity { get; set; }

        /// <summary>
        /// Disponibilidad del producto (disponible o vendido).
        /// </summary>
        public required Availability Availability { get; set; }

        /// <summary>
        /// Estado del producto (nuevo o usado).
        /// </summary>
        public required Condition Condition { get; set; }
    }
}
