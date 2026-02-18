using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Options;

namespace backend.src.Application.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<string> CreateOfferAsync(CreateOfferDTO publicationDTO, int userId);
        Task<string> CreateBuySellAsync(CreateBuySellDTO publicationDTO, int userId);

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

        Task<PublicationsForAdminDTO> GetAllPublicationsForAdminAsync(
            int adminId,
            PublicationsForAdminSearchParamsDTO searchParamsDTO
        );

        Task<PublicationDetailsForAdminDTO> GetPublicationDetailsForAdminByIdAsync(
            int publicationId,
            int adminId
        );

        Task<string> AppealRejectedPublicationAsync(
            int publicationId,
            int offerorId,
            AppealRejectionDTO appealDTO
        );

        /// <summary>
        /// Permite al oferente cerrar manualmente una oferta que está en estado "Realizando Trabajo", o "Recibiendo Postulaciones".
        /// El objetivo del metodo es avanzar la postulacion un estado hacia adelante si se encuentra en uno de estos dos estados.
        /// </summary>
        /// <param name="publicationId">Id de la publicación a cerrar manualmente</param>
        /// <param name="requestingUserId">Id del usuario que solicita el cierre manual</param>
        /// <returns>Mensaje de éxito o error</returns>
        Task<string> AdvanceOfferManuallyAsync(int publicationId, int requestingUserId);

        /// <summary>
        /// Permite al oferente (o a un administrador) cancelar manualmente una oferta antes de que se cierre para postulaciones, cambiando su estado a "CanceladaAntesDelTrabajo" y evitando que sea visible para usuarios regulares.
        /// Esto es útil para casos en los que el oferente ya no puede cumplir con la oferta o desea retirarla por cualquier motivo antes de que se cierre oficialmente.
        /// A diferencia del cierre manual estándar, esta acción no activa el flujo de reseñas ni calificaciones, ya que la oferta no llega a la etapa de realización del trabajo o voluntariado.
        /// </summary>
        /// <param name="publicationId">Id de la publicación a cancelar manualmente</param>
        /// <param name="requestingUserId">Id del usuario que solicita la cancelación manual</param>
        /// <param name="requestDTO">Datos adicionales de la solicitud de cancelación (opcional)</param>
        /// <returns>Mensaje de éxito o error</returns>
        Task<string> CancelOfferManuallyAsync(
            int publicationId,
            int requestingUserId,
            ClosePublicationRequestDTO? requestDTO = null
        );
        #endregion
    }
}
