using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.ApplicantsForAdminDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Options;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    public interface IOfferApplicationRepository
    {
        Task<bool> AddAsync(JobApplication application);
        Task<JobApplication?> GetByIdAsync(
            int applicationId,
            JobApplicationOptions? options = null
        );
        Task<JobApplication?> GetByStudentAndOfferAsync(int studentId, int offerId);
        Task<(IEnumerable<JobApplication>, int)> GetByApplicantIdFilteredAsync(
            int applicantId,
            SearchParamsDTO searchParams
        );
        Task<(IEnumerable<JobApplication>, int)> GetAllByOfferIdAsync(
            int offerId,
            ApplicationsForOfferorSearchParamsDTO searchParams
        );
        Task<(IEnumerable<JobApplication>, int)> GetAllByOfferIdForAdminAsync(
            int offerId,
            ApplicationsForAdminSearchParamsDTO searchParams
        );
        Task<IEnumerable<JobApplication>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<JobApplication>> GetByOfferIdAsync(int offerId);
        Task<bool> UpdateAsync(JobApplication application);
        Task<bool> ExistsByApplicantIdAndOfferId(int applicantId, int offerId);
        Task<bool> HasPendingCvRequiredApplication(int applicantId);
        Task<bool> MarkCvAsInvalidAsync(int applicantId);
    }
}
