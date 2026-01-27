using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Domain.Models;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<bool> AddAsync(JobApplication application);
        Task<JobApplication?> GetByIdAsync(int applicationId);
        Task<JobApplication?> GetByStudentAndOfferAsync(int studentId, int offerId);
        Task<(IEnumerable<JobApplication>, int)> GetByApplicantIdFilteredAsync(
            int applicantId,
            SearchParamsDTO searchParams
        );
        Task<IEnumerable<JobApplication>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<JobApplication>> GetByOfferIdAsync(int offerId);
        Task<bool> UpdateAsync(JobApplication application);
        Task<bool> ExistsByApplicantIdAndOfferId(int applicantId, int offerId);
    }
}
