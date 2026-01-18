using backend.src.Application.DTOs.OfferDTOs;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces;

/// <summary>
/// Service interface that exposes operations related to offers.
/// Implementations handle business logic for retrieving, publishing,
/// rejecting and administrating offers.
/// </summary>
public interface IOfferService
{
    Task<Offer> GetByOfferIdAsync(int offerId);

    /// <summary>
    /// Retrieves detailed information for a single offer by its identifier.
    /// Returns null if the offer is not found.
    /// </summary>
    /// <param name="offerId">Offer identifier.</param>
    /// <returns>An <see cref="OfferDetailDto"/> or null.</returns>
    Task<OfferDetailDto?> GetOfferDetailsAsync(int offerId);

    /// <summary>
    /// Gets all active offers available to users.
    /// </summary>
    /// <returns>Sequence of <see cref="OfferSummaryDto"/> representing active offers.</returns>
    Task<IEnumerable<OfferSummaryDto>> GetActiveOffersAsync();

    /// <summary>
    /// Marks an offer as published/active.
    /// </summary>
    /// <param name="id">Offer identifier to publish.</param>
    Task PublishOfferAsync(int id);

    /// <summary>
    /// Marks an offer as rejected/inactive.
    /// </summary>
    /// <param name="id">Offer identifier to reject.</param>
    Task RejectOfferAsync(int id);

    /// <summary>
    /// Returns offers that are pending administrative review.
    /// </summary>
    /// <returns>Sequence of pending offers DTOs for admin use.</returns>
    Task<IEnumerable<PendingOffersForAdminDto>> GetPendingOffersAsync();

    /// <summary>
    /// Retrieves offers that have already been published.
    /// </summary>
    /// <returns>Sequence of published offer DTOs for admin listing.</returns>
    Task<IEnumerable<OfferBasicAdminDto>> GetPublishedOffersAsync();

    /// <summary>
    /// Gets detailed information required by administrators to manage a specific offer.
    /// </summary>
    /// <param name="offerId">Offer identifier.</param>
    /// <returns>Admin-focused offer details DTO.</returns>
    Task<OfferDetailsAdminDto> GetOfferDetailsForAdminManagement(int offerId);

    /// <summary>
    /// Closes (deactivates) an offer from the admin perspective.
    /// </summary>
    /// <param name="offerId">Offer identifier to close.</param>
    Task GetOfferForAdminToClose(int offerId);

    /// <summary>
    /// Retrieves data needed to validate an offer (used in validation flows).
    /// </summary>
    /// <param name="id">Offer identifier.</param>
    /// <returns>Offer validation details DTO.</returns>
    Task<OfferDetailValidationDto> GetOfferDetailForOfferValidationAsync(int id);

    /// <summary>
    /// Publishes an offer as part of the admin approval workflow.
    /// </summary>
    /// <param name="id">Offer identifier to publish.</param>
    Task GetOfferForAdminToPublish(int id);

    /// <summary>
    /// Rejects an offer as part of the admin approval workflow.
    /// </summary>
    /// <param name="id">Offer identifier to reject.</param>
    Task GetOfferForAdminToReject(int id);
    Task<OfferDetailDto> GetOfferDetailForOfferer(int id, string userId);
    Task ClosePublishedOfferAsync(int offerId);

    /// <summary>
    /// Cierra (desactiva) una oferta publicada. Solo el propietario puede usarlo.
    /// </summary>
    /// <param name="offerId">Identificador de la oferta.</param>
    /// <param name="offererUserId">Identificador del usuario oferente (propietario).</param>
    Task ClosePublishedOfferForOffererAsync(int offerId, int offererUserId);
}
