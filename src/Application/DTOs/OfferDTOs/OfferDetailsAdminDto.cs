using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.OfferDTOs
{
    /// <summary>
    /// <summary>
    /// DTO with offer information surfaced to administrators for review and management.
    /// </summary>
    public class OfferDetailsAdminDto
    {
        /// <summary>
        /// Offer title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Full description of the offer.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Collection of image URLs attached to the offer.
        /// </summary>
        public required ICollection<string> Images { get; set; }

        /// <summary>
        /// Company or owner name who posted the offer.
        /// </summary>
        public required string CompanyName { get; set; }

        /// <summary>
        /// Date when the offer was published.
        /// </summary>
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Publication type (Offer, BuySell).
        /// </summary>
        public PublicationType Type { get; set; }

        /// <summary>
        /// Whether the offer is currently active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Administrative validation status for the offer.
        /// </summary>
        public ApprovalStatus statusValidation { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DeadlineDate { get; set; }
        public int Remuneration { get; set; }

        /// <summary>
        /// Offer type enum from the domain model.
        /// </summary>
        public OfferTypes OfferType { get; set; }
        public string Location { get; set; }
        public string Requirements { get; set; }
        public string ContactInfo { get; set; }
        public string AboutMe { get; set; }
        public double Rating { get; set; }
    }
}
