using backend.src.Application.DTOs.PublicationDTO.CreatePublicationDTOs;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.BuySells;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.SpecificUserPublicationsDTO;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Domain.Models;

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

        Task<UserPublicationsForAdminDTO> GetPublicationByUserIdAsync(
            int adminId,
            int userId,
            UserPublicationsSearchParamsDTO searchParams
        );

        Task<OffersForApplicantDTO> GetOffersAsync(
            ExploreOffersSearchParamsDTO searchParams,
            int? userId = null
        );

        Task<BuySellsForApplicantDTO> GetBuySellsAsync(
            ExploreBuySellsSearchParamsDTO searchParams,
            int? userId = null
        );

        Task<OfferDetailsForApplicantDTO> GetOfferDetailsForApplicantAsync(
            int publicationId,
            int applicantId
        );

        Task<BuySellDetailsForApplicantDTO> GetBuySellDetailsForApplicantAsync(
            int publicationId,
            int applicantId
        );

        Task<OfferDetailsForPublicDTO> GetOfferDetailsForPublicAsync(int publicationId);

        Task<BuySellDetailsForPublicDTO> GetBuySellDetailsForPublicAsync(int publicationId);

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

        Task<string> CancelPublicationManuallyAsync(
            int publicationId,
            int requestingUserId,
            ClosePublicationRequestDTO? requestDTO = null
        );

        /// <summary>
        /// Permite al oferente cerrar manualmente una oferta que está en estado "Realizando Trabajo", o "Recibiendo Postulaciones".
        /// Este metodo llama a los mismos metodos llamados por los trabajos programados para cerrar las postulaciones, finalizar el trabajo e inicializar las reseñas, y finalizar las reseñas, dependiendo del estado actual de la publicación, pero lo hace de forma inmediata al cancelar los trabajos programados correspondientes y ejecutando manualmente los métodos necesarios para avanzar la publicación al siguiente estado.
        /// </summary>
        /// <param name="publicationId">Id de la publicación a cerrar manualmente</param>
        /// <param name="requestingUserId">Id del usuario que solicita el cierre manual</param>
        /// <returns>Mensaje de éxito o error</returns>
        Task<string> AdvanceOfferManuallyAsync(int publicationId, int requestingUserId);

        Task<string> EditBuySellDetailsAsync(
            int publicationId,
            int applicantId,
            EditMyBuySellDetailsDTO detailsDTO
        );

        Task<string> ToggleBuySellVisibilityAsync(int publicationId, int offerorId);
        #endregion

        #region Background Jobs

        /// <summary>
        /// Cierra una oferta para nuevas postulaciones, cambiando su estado a "CerradaParaPostulaciones" y evitando que nuevos usuarios puedan postularse.
        /// Resuelve tambien algunos casos especiales: Si la oferta no tiene postulantes aceptados, se marca como "CanceladaAntesDelTrabajo". Si la oferta aun esta en "Pendiente", se marca como "Rechazada" para evitar que quede en un estado inconsistente.
        /// </summary>
        /// <param name="offerId">Id de la oferta a cerrar para postulaciones</param>
        /// <returns></returns>
        Task CloseOfferForApplicationsAsync(int offerId);

        /// <summary>
        /// Marca una oferta como "Realizando Trabajo" y crea las reseñas correspondientes para el proceso de evaluación, permitiendo que los usuarios que postularon puedan dejar sus reseñas una vez que la oferta se haya cerrado para postulaciones.
        /// </summary>
        /// <param name="offerId">Id de la oferta a marcar como "Realizando Trabajo"</param>
        /// <returns></returns>
        Task CompleteAndInitializeReviewsAsync(int offerId);

        /// <summary>
        /// Marca una oferta como "Finalizada" y cierra las reseñas correspondientes, evitando que los usuarios puedan dejar nuevas reseñas después de un tiempo definido desde el cierre de la oferta.
        /// </summary>
        /// <param name="offerId">Id de la oferta a marcar como "Finalizada"</param>
        /// <returns></returns>
        Task FinalizeAndCloseReviewsAsync(int offerId);

        #endregion
    }
}
