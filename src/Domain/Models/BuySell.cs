namespace backend.src.Domain.Models
{
    /// <summary>
    /// Disponibilidad del producto (disponible o vendido).
    /// </summary>
    public enum Availability
    {
        Disponible,
        Vendido,
        Cerrado,
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

    public enum Category
    {
        Electronica,
        Ropa,
        Hogar,
        Vehiculos,
        Deportes,
        Libros,
        Musica,
        Juguetes,
        Mascotas,
        Servicios,
        Otros,
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
        public required Category Category { get; set; }

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

        /// <summary>
        /// Indica si el oferente ha optado por mostrar su correo electrónico de contacto en la publicación.
        /// </summary>
        public required bool IsEmailAvailable { get; set; }

        /// <summary>
        /// Indica si el oferente ha optado por mostrar su número de teléfono de contacto en la publicación.
        /// </summary>
        public required bool IsPhoneAvailable { get; set; }

        // === Atributos computados para facilitar consultas ===
        public bool IsAvailable => Availability == Availability.Disponible && Quantity > 0;
    }
}
