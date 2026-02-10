using backend.src.Application.DTOs.JobAplicationDTO;
using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs.ApplicationsForOfferorDTOs;
using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de postulaciones a ofertas laborales
    /// </summary>
    public interface IOfferApplicationService
    {
        Task<string> CreateApplicationAsync(int studentId, int offerId, CoverLetterDTO coverLetter);

        Task<ApplicationsForApplicantDTO> GetApplicationsByUserIdAsync(
            int userId,
            SearchParamsDTO searchParams
        );

        Task<ApplicationsForOfferorDTO> GetAllApplicationsByOfferIdAsync(
            int offerId,
            int offererId,
            ApplicationsForOfferorSearchParamsDTO searchParams
        );

        Task<GetApplicationDetailsDTO> GetApplicationDetailsForApplicantAsync(
            int userId,
            int applicationId
        );

        Task<string> UpdateApplicationDetailsAsync(
            int userId,
            int applicationId,
            UpdateApplicationDetailsDTO updateDto
        );

        Task<string> UpdateApplicationStatusByOfferorAsync(
            int offererId,
            int applicationId,
            int offerId,
            UpdateApplicationStatusDTO newStatus
        );
    }
}
