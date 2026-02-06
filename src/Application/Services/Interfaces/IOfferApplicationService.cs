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
        //! COMPLETE
        /// <summary>
        /// Crea una nueva postulación para una oferta laboral
        /// </summary>
        /// <param name="studentId">ID del estudiante que postula</param>
        /// <param name="offerId">ID de la oferta laboral</param>
        /// <returns>Información de la postulación creada</returns>
        Task<string> CreateApplicationAsync(int studentId, int offerId, CoverLetterDTO coverLetter);

        //! COMPLETE
        /// <summary>
        /// Obtiene todas las postulaciones realizadas por un estudiante específico
        /// </summary>
        /// <param name="userId">ID del estudiante</param>
        /// <returns>Lista de postulaciones del estudiante</returns>
        Task<ApplicationsForApplicantDTO> GetApplicationsByUserIdAsync(
            int userId,
            SearchParamsDTO searchParams
        );

        Task<ApplicationsForOfferorDTO> GetAllApplicationsByOfferIdAsync(
            int offerId,
            int offererId,
            ApplicationsForOfferorSearchParamsDTO searchParams
        );

        //! COMPLETE
        /// <summary>
        /// Obtiene los detalles de una postulación específica para un estudiante.
        /// </summary>
        /// <param name="userId">ID del estudiante</param>
        /// <param name="applicationId">ID de la postulación</param>
        /// <returns>Detalles de la postulación</returns>
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

        Task<bool> UpdateApplicationStatusAsync(
            int applicationId,
            ApplicationStatus newStatus,
            int companyId
        );

        Task<IEnumerable<ViewApplicantsDto>> GetApplicantsForAdminManagement(int offerId);

        Task<ViewApplicantDetailAdminDto> GetApplicantDetailForAdmin(int studentId);

        Task<IEnumerable<OffererApplicantViewDto>> GetApplicantsForOffererAsync(
            int offerId,
            int offererUserId
        );

        Task<ViewApplicantUserDetailDto> GetApplicantDetailForOfferer(
            int studentId,
            int offerId,
            int offererUserId
        );
    }
}
