using backend.src.Application.DTOs.JobAplicationDTO;
using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de postulaciones a ofertas laborales
    /// </summary>
    public interface IJobApplicationService
    {
        /// <summary>
        /// Crea una nueva postulación para una oferta laboral
        /// </summary>
        /// <param name="studentId">ID del estudiante que postula</param>
        /// <param name="offerId">ID de la oferta laboral</param>
        /// <returns>Información de la postulación creada</returns>
        Task<string> CreateApplicationAsync(int studentId, int offerId);

        /// <summary>
        /// Obtiene todas las postulaciones realizadas por un estudiante específico
        /// </summary>
        /// <param name="userId">ID del estudiante</param>
        /// <returns>Lista de postulaciones del estudiante</returns>
        //? LEGACY RETURN TYPE: IEnumerable<JobApplicationResponseDto>
        Task<ApplicationsForApplicantDTO> GetUserApplicationsByIdAsync(
            int userId,
            SearchParamsDTO searchParams
        );

        //!
        Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByOfferIdAsync(int offerId);

        Task<JobApplicationDetailDto?> GetApplicationDetailAsync(int applicationId);

        //? LEGACY METHOD Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByCompanyIdAsync(int companyId);

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

        /**
        * ? LEGACY METHOD FOR COMPATIBILTY
        */
        Task<IEnumerable<JobApplicationResponseDto>> GetStudentApplicationsAsync(int studentId);
    }
}
