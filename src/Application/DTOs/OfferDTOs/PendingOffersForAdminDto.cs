using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.OfferDTOs
{
    /// <summary>
    /// DTO containing minimal information about offers pending admin validation.
    /// Used to populate admin review lists.
    /// </summary>
    public class PendingOffersForAdminDto
    {
        /// <summary>
        /// Offer title.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Offer title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Full offer description.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Location of the offer (optional).
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Publication date of the offer.
        /// </summary>
        public DateTime PostDate { get; set; }

        /// <summary>
        /// Remuneration offered (integer value).
        /// </summary>
        public required int Remuneration { get; set; }

        /// <summary>
        /// Offer type enum from the domain model.
        /// </summary>
        public OfferTypes OfferType { get; set; }
    }
}
