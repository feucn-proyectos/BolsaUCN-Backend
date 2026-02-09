namespace backend.src.Domain.Models
{
    /// <summary>
    /// Enum que define los tipos de publicaciones disponibles en la plataforma. Usado para diferenciar entre ofertas laborales/voluntariados y anuncios de compra/venta, permitiendo manejar lógicas específicas para cada tipo de publicación en el sistema.
    /// </summary>
    public enum PublicationType
    {
        Oferta, // Oferta de trabajo o voluntariado
        CompraVenta, // Anuncio de compra/venta
    }

    /// <summary>
    /// Enum que define el estado de aprobación administrativa de una publicación. Usado para controlar el flujo de revisión y publicación de las ofertas y anuncios en la plataforma.
    /// </summary>
    public enum ApprovalStatus
    {
        Aceptada, // Aprobado y publicado por un administrador
        Pendiente, // En revisión administrativa
        Rechazada, // Rechazado por un administrador
        Cerrada, // Cerrado por el usuario o administrador, o automaticamente cuando llega a la fecha de finalización.
    }

    /// <summary>
    /// Enum que define la visibilidad de una publicación, usada pricipalmente por administradores para manejar casos de publicaciones que necesitan ser ocultadas temporalmente por motivos administrativos
    /// o por oferentes que quieren guardar borradores de sus publicaciones sin que sean visibles para otros usuarios. Esto solo maneja visibilidad, en caso de ofertas la logica de aprovacion, apelacion, y reseñas sigue activa.
    /// </summary>
    public enum Visibility
    {
        Publica, // Visible para todos los usuarios
        Privada, // Visible solo para el usuario que creó la publicación (usada principalmente para borradores)
    }

    /// <summary>
    /// Clase base abstracta para todas las entidades de publicación en el sistema.
    /// Los tipos derivados incluyen <see cref="Offer"/> y <see cref="BuySell"/>.
    /// </summary>
    public abstract class Publication : ModelBase
    {
        /// <summary>
        /// El usuario que creó la publicación.
        /// </summary>
        public required User User { get; set; }

        /// <summary>
        /// Identificador del usuario que creó la publicación.
        /// </summary>
        public required int UserId { get; set; }

        /// <summary>
        /// Título de la publicación.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Descripción completa de la publicación.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Ubicación asociada a la publicación.
        /// </summary>
        public required string Location { get; set; }

        /// <summary>
        /// Información de contacto adicional proporcionada por el usuario.
        /// </summary>
        public string? AdditionalContactEmail { get; set; }

        /// <summary>
        /// Número de teléfono de contacto adicional proporcionado por el usuario.
        /// </summary>
        public string? AdditionalContactPhoneNumber { get; set; }

        /// <summary>
        /// Tipo de publicación (Oferta, CompraVenta).
        /// </summary>
        public required PublicationType PublicationType { get; set; }

        /// <summary>
        /// Estado de validación administrativa de la publicación.
        /// </summary>
        public ApprovalStatus ApprovalStatus { get; set; }

        /// <summary>
        /// Razón de rechazo proporcionada por el administrador.
        /// Esta retroalimentación permite al usuario corregir su publicación.
        /// </summary>
        public string? AdminRejectionReason { get; set; }

        /// <summary>
        /// Contador del número de intentos de apelación realizados por el usuario.
        /// Usado para hacer cumplir la lógica del límite máximo de apelaciones.
        /// </summary>
        public int AppealCount { get; set; } = 0;
    }
}
