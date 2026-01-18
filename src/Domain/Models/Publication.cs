namespace backend.src.Domain.Models
{
    /// <summary>
    /// Enum that defines types of publications in the system.
    /// </summary>
    public enum Types
    {
        Oferta, // Oferta de trabajo o voluntariado
        CompraVenta, // Anuncio de compra/venta
    }

    /// <summary>
    /// Validation state used by administrative workflows.
    /// </summary>
    public enum StatusValidation
    {
        Publicado, // Validado y publicado por un administrador
        EnProceso, // En revisión administrativa
        Rechazado, // Rechazado por un administrador
        Cerrado, // Cerrado por el usuario o administrador
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
        public string? AdditionalContactInfo { get; set; }

        /// <summary>
        /// Coleccion de imágenes asociadas a la publicación.
        /// </summary>
        public ICollection<Image> Images { get; set; } = new List<Image>();

        /// <summary>
        /// Tipo de publicación (Oferta, CompraVenta).
        /// </summary>
        public required Types Type { get; set; }

        /// <summary>
        /// Indica si la publicación está activa y visible para los usuarios.
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// Estado de validación administrativa de la publicación.
        /// </summary>
        public StatusValidation StatusValidation { get; set; }

        /// <summary>
        /// Razón de rechazo proporcionada por el administrador.
        /// Esta retroalimentación permite al usuario corregir su publicación.
        /// </summary>
        public string? AdminRejectionReason { get; set; }

        /// <summary>
        /// Justificación proporcionada por el usuario al apelar un rechazo.
        /// </summary>
        public string? UserAppealJustification { get; set; }

        /// <summary>
        /// Contador del número de intentos de apelación realizados por el usuario.
        /// Usado para hacer cumplir la lógica del límite máximo de apelaciones.
        /// </summary>
        public int AppealCount { get; set; } = 0;
    }
}
