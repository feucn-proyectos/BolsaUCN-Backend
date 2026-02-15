namespace backend.src.Application.DTOs.OfferDTOs
{
    /// <summary>
    /// DTO for basic offer information used when sensitive contact details should be hidden.
    /// Intended for non-student users or public listings.
    /// </summary>
    public class OfferBasicDto
    {
        /// <summary>
        /// Offer identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Title or position name.
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Short description of the offer.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Location of the offer.
        /// </summary>
        public string Ubicacion { get; set; } = string.Empty;

        /// <summary>
        /// Remuneration amount.
        /// </summary>
        public int Remuneracion { get; set; }

        /// <summary>
        /// Offer type (as string).
        /// </summary>
        public string TipoOferta { get; set; } = string.Empty;

        /// <summary>
        /// Publication date.
        /// </summary>
        public DateTime FechaPublicacion { get; set; }

        /// <summary>
        /// End date for the offer.
        /// </summary>
        public DateTime FechaTermino { get; set; }

        /// <summary>
        /// Whether the offer is active.
        /// </summary>
        public bool Activa { get; set; }

        // NOTE: ContactInfo and Requirements are intentionally excluded to protect sensitive data.
    }
}
