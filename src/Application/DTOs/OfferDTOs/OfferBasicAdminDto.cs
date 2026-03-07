using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.OfferDTOs
{
    /// <summary>
    /// DTO used by administrators when listing and managing published offers.
    /// </summary>
    public class OfferBasicAdminDto
    {
        public int Id { get; set; }

        /// <summary>
        /// Offer title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Name of the company or individual that posted the offer.
        /// </summary>
        public required string CompanyName { get; set; }

        /// <summary>
        /// Publication date of the offer.
        /// </summary>
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Offer type (domain enum).
        /// </summary>
        public OfferTypes OfferType { get; set; }

        /// <summary>
        /// Whether the offer is active.
        /// </summary>
        public bool Activa { get; set; }

        public int Remuneration { get; set; }
    }
}
