using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<string> CreateOfferAsync(CreateOfferDTO publicationDTO, int userId);
        Task<string> CreateBuySellAsync(CreateBuySellDTO publicationDTO, int userId);

        /// <summary>
        /// Funcion
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<PublicationsDTO>> GetMyPublishedPublicationsAsync(string userId);
        Task<IEnumerable<PublicationsDTO>> GetMyRejectedPublicationsAsync(string userId);
        Task<IEnumerable<PublicationsDTO>> GetMyPendingPublicationsAsync(string userId);

        Task<GenericResponse<string>> AdminApprovePublicationAsync(int publicationId);

        Task<GenericResponse<string>> AdminRejectPublicationAsync(
            int publicationId,
            AdminRejectDto dto
        );
        Task<GenericResponse<string>> AppealPublicationAsync(
            int publicationId,
            int userId,
            UserAppealDto dto
        );

        Task<Publication?> GetPublicationByIdAsync(
            int publicationId,
            PublicationQueryOptions options
        );
    }
}
