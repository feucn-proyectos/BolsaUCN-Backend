using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Options;

namespace backend.src.Application.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<string> CreateOfferAsync(CreateOfferDTO publicationDTO, int userId);
        Task<string> CreateBuySellAsync(CreateBuySellDTO publicationDTO, int userId);
        Task<GenericResponse<string>> AppealPublicationAsync(int publicationId, int userId);

        #region New Methods

        /// <summary>
        /// Obtiene las publicaciones del usuario autenticado
        /// </summary>
        /// <param name="userId">Id del usuario autenticado</param>
        /// <returns>Lista de publicaciones del usuario</returns>
        Task<MyPublicationsDTO> GetMyPublicationsAsync(
            int userId,
            MyPublicationsSeachParamsDTO searchParamsDTO
        );

        Task<OffersForApplicantDTO> GetOffersAsync(
            ExploreOffersSearchParamsDTO searchParams,
            int? userId = null
        );

        Task<OfferDetailsForApplicantDTO> GetOfferDetailsForApplicantAsync(
            int publicationId,
            int applicantId
        );

        Task<OfferDetailsForPublicDTO> GetOfferDetailsForPublicAsync(int publicationId);

        /// <summary>
        /// Obtiene los detalles de una publicación específica del usuario autenticado
        /// </summary>
        /// <param name="publicationId">Id de la publicación</param>
        /// <param name="userId">Id del usuario autenticado</param>
        /// <returns>Detalles de la publicación</returns>
        Task<MyPublicationDetailsDTO> GetPublicationDetailsForOffererAsync(
            int publicationId,
            int offerorId
        );
        Task UpdatePublicationStatusAsync(
            Publication publication,
            ApprovalStatus newStatus,
            string? rejectionReason = null
        );

        Task<string> CloseOfferManuallyAsync(int publicationId, int requestingUserId);
        #endregion
    }
}
