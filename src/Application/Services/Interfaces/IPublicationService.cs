using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Options;

namespace backend.src.Application.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<string> CreateOfferAsync(CreateOfferDTO publicationDTO, int userId);
        Task<string> CreateBuySellAsync(CreateBuySellDTO publicationDTO, int userId);
        Task<IEnumerable<PublicationsDTO>> GetMyPublishedPublicationsAsync(string userId);
        Task<IEnumerable<PublicationsDTO>> GetMyRejectedPublicationsAsync(string userId);
        Task<IEnumerable<PublicationsDTO>> GetMyPendingPublicationsAsync(string userId);
        Task<GenericResponse<string>> AppealPublicationAsync(
            int publicationId,
            int userId,
            UserAppealDto dto
        );

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
        Task UpdatePublicationStatusAsync(Publication publication, ApprovalStatus newStatus);
        #endregion
    }
}
