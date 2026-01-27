namespace backend.src.Application.DTOs.OfferDTOs;

using backend.src.Domain.Models;

/// <summary>
/// DTO representing a compact summary of an offer used in listings.
/// </summary>
public class OfferSummaryDto
{
    /// <summary>
    /// Offer identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Offer title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Name of the company or individual who posted the offer.
    /// </summary>
    public required string CompanyName { get; set; }

    /// <summary>
    /// Optional location information for the offer.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Offered remuneration (currency units handled by the caller).
    /// </summary>
    public decimal Remuneration { get; set; }

    /// <summary>
    /// Optional application deadline date.
    /// </summary>
    public DateTime? DeadlineDate { get; set; }

    /// <summary>
    /// Date when the offer was published.
    /// </summary>
    public DateTime PublicationDate { get; set; }

    /// <summary>
    /// Offer type enum from the domain model.
    /// </summary>
    public OfferTypes OfferType { get; set; }

    /// <summary>
    /// Formatted owner name (optional convenience field).
    /// </summary>
    public string OwnerName { get; set; } = "UCN";
    public double OwnerRating { get; set; }
}

/// <summary>
/// DTO containing detailed information about an offer.
/// </summary>
public class OfferDetailDto
{
    /// <summary>
    /// Offer identifier.
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
    /// Name of the company or individual who posted the offer.
    /// </summary>
    public required string CompanyName { get; set; }

    /// <summary>
    /// Location of the offer (optional).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Publication date of the offer.
    /// </summary>
    public DateTime PostDate { get; set; }

    /// <summary>
    /// End date for the offer.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Remuneration offered (integer value).
    /// </summary>
    public required int Remuneration { get; set; }

    /// <summary>
    /// Offer type as a string (e.g., "Trabajo", "Voluntariado").
    /// </summary>
    public required string OfferType { get; set; }

    public required ApprovalStatus statusValidation { get; set; }
}
