using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.ApplicantsForAdminDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Infrastructure.Repositories.Implements
{
    public class OfferApplicationRepository : IOfferApplicationRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public OfferApplicationRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

        public async Task<bool> AddAsync(JobApplication application)
        {
            _context.JobApplications.Add(application);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<JobApplication?> GetByIdAsync(
            int applicationId,
            JobApplicationOptions? options = null
        )
        {
            var query = _context.JobApplications.AsQueryable();

            if (options != null)
            {
                if (!options.TrackChanges)
                {
                    query = query.AsNoTracking();
                }
                if (options.IncludeStudent)
                {
                    query = query.Include(ja => ja.Student).ThenInclude(s => s!.CV);
                }
                if (options.IncludeJobOffer)
                {
                    query = query
                        .Include(ja => ja.JobOffer)
                        .ThenInclude(jo => jo!.User)
                        .ThenInclude(u => u.ProfilePhoto);
                }
            }

            return await query.FirstOrDefaultAsync(ja => ja.Id == applicationId);
        }

        public async Task<JobApplication?> GetByStudentAndOfferAsync(int studentId, int offerId)
        {
            return await _context
                .JobApplications.Include(ja => ja.Student)
                .Include(ja => ja.JobOffer)
                .ThenInclude(jo => jo!.User)
                .FirstOrDefaultAsync(ja => ja.StudentId == studentId && ja.JobOfferId == offerId);
        }

        public async Task<(IEnumerable<JobApplication>, int)> GetByApplicantIdFilteredAsync(
            int applicantId,
            SearchParamsDTO searchParams
        )
        {
            var query = _context
                .JobApplications.Include(ja => ja.JobOffer)
                .ThenInclude(jo => jo!.User)
                .Include(ja => ja.Student)
                .Where(ja => ja.StudentId == applicantId)
                .AsQueryable();

            // Aplicar filtros de búsqueda si es necesario
            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                string term = searchParams.SearchTerm.ToLower();
                query = query.Where(ja =>
                    ja.JobOffer!.Title.ToLower().Contains(term)
                    || ja.JobOffer.User.FirstName.ToLower().Contains(term)
                    || ja.JobOffer.User.LastName.ToLower().Contains(term)
                );
            }

            // Aplicar filtro por estado
            if (!string.IsNullOrEmpty(searchParams.StatusFilter))
            {
                if (
                    Enum.TryParse(
                        searchParams.StatusFilter,
                        ignoreCase: true,
                        out ApplicationStatus statusEnum
                    )
                )
                {
                    query = query.Where(u => u.Status == statusEnum);
                }
            }

            // Aplicar ordenamiento
            if (!string.IsNullOrEmpty(searchParams.SortBy))
            {
                bool ascending = searchParams.SortOrder?.ToLower() == "asc";
                switch (searchParams.SortBy)
                {
                    case "OfferTitle":
                        query = ascending
                            ? query.OrderBy(ja => ja.JobOffer!.Title)
                            : query.OrderByDescending(ja => ja.JobOffer!.Title);
                        break;
                    case "CreatedAt":
                        query = ascending
                            ? query.OrderBy(ja => ja.CreatedAt)
                            : query.OrderByDescending(ja => ja.CreatedAt);
                        break;
                }
            }
            else // Orden por defecto
            {
                query = query.OrderByDescending(ja => ja.CreatedAt);
            }

            int totalCount = await query.CountAsync();
            int pageNumber = searchParams.PageNumber > 0 ? searchParams.PageNumber : 1;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;

            var applications = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (applications, totalCount);
        }

        public async Task<(IEnumerable<JobApplication>, int)> GetAllByOfferIdAsync(
            int offerId,
            ApplicationsForOfferorSearchParamsDTO searchParams
        )
        {
            var query = _context
                .JobApplications.Include(ja => ja.Student)
                .ThenInclude(p => p!.ProfilePhoto)
                .Include(ja => ja.Student)
                .ThenInclude(c => c!.CV)
                .Include(ja => ja.JobOffer)
                .Where(ja =>
                    ja.JobOfferId == offerId
                    && ja.Status != ApplicationStatus.CanceladaPorPostulante
                );

            // Aplicar ordenamiento
            if (!string.IsNullOrEmpty(searchParams.SortBy))
            {
                bool ascending = searchParams.SortOrder?.ToLower() == "asc";
                switch (searchParams.SortBy)
                {
                    case "FirstName":
                        query = ascending
                            ? query
                                .OrderBy(ja => ja.Student!.FirstName)
                                .ThenBy(ja => ja.Student!.LastName)
                            : query
                                .OrderByDescending(ja => ja.Student!.FirstName)
                                .ThenByDescending(ja => ja.Student!.LastName);
                        break;
                    case "ApplicationDate":
                        query = ascending
                            ? query.OrderBy(ja => ja.CreatedAt)
                            : query.OrderByDescending(ja => ja.CreatedAt);
                        break;
                }
            }
            else // Orden por defecto
            {
                query = query.OrderByDescending(ja => ja.CreatedAt);
            }

            int totalCount = await query.CountAsync();
            int pageNumber = searchParams.PageNumber > 0 ? searchParams.PageNumber : 1;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;

            var applications = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (applications, totalCount);
        }

        public async Task<IEnumerable<JobApplication>> GetByStudentIdAsync(int studentId)
        {
            return await _context
                .JobApplications.Include(ja => ja.JobOffer)
                .Include(ja => ja.Student)
                .Where(ja => ja.StudentId == studentId)
                .OrderByDescending(ja => ja.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobApplication>> GetByOfferIdAsync(int offerId)
        {
            return await _context
                .JobApplications.Include(ja => ja.Student)
                .Include(ja => ja.JobOffer)
                .Where(ja => ja.JobOfferId == offerId)
                .OrderByDescending(ja => ja.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(JobApplication application)
        {
            _context.JobApplications.Update(application);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsByApplicantIdAndOfferId(int applicantId, int offerId)
        {
            return await _context.JobApplications.AnyAsync(ja =>
                ja.StudentId == applicantId && ja.JobOfferId == offerId
            );
        }

        public async Task<(IEnumerable<JobApplication>, int)> GetAllByOfferIdForAdminAsync(
            int offerId,
            ApplicationsForAdminSearchParamsDTO searchParams
        )
        {
            var query = _context
                .JobApplications.Include(ja => ja.Student)
                .ThenInclude(p => p!.ProfilePhoto)
                .Include(ja => ja.Student)
                .ThenInclude(c => c!.CV)
                .Include(ja => ja.JobOffer)
                .Where(ja => ja.JobOfferId == offerId)
                .AsQueryable();

            // Aplicar ordenamiento
            if (!string.IsNullOrEmpty(searchParams.SortBy))
            {
                bool ascending = searchParams.SortOrder?.ToLower() == "asc";
                switch (searchParams.SortBy)
                {
                    case "FirstName":
                        query = ascending
                            ? query
                                .OrderBy(ja => ja.Student!.FirstName)
                                .ThenBy(ja => ja.Student!.LastName)
                            : query
                                .OrderByDescending(ja => ja.Student!.FirstName)
                                .ThenByDescending(ja => ja.Student!.LastName);
                        break;
                    case "ApplicationDate":
                        query = ascending
                            ? query.OrderBy(ja => ja.CreatedAt)
                            : query.OrderByDescending(ja => ja.CreatedAt);
                        break;
                    case "Status":
                        query = ascending
                            ? query.OrderBy(ja => ja.Status)
                            : query.OrderByDescending(ja => ja.Status);
                        break;
                }
            }
            else // Orden por defecto
            {
                query = query.OrderByDescending(ja => ja.CreatedAt);
            }

            int totalCount = await query.CountAsync();
            int pageNumber = searchParams.PageNumber > 0 ? searchParams.PageNumber : 1;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;

            var applications = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (applications, totalCount);
        }

        public async Task<bool> HasPendingCvRequiredApplication(int applicantId)
        {
            return await _context.JobApplications.AnyAsync(ja =>
                ja.StudentId == applicantId
                && ja.Status == ApplicationStatus.Pendiente
                && ja.JobOffer!.IsCvRequired
            );
        }

        public async Task<bool> MarkCvAsInvalidAsync(int applicantId)
        {
            var applicationsToUpdate = await _context
                .JobApplications.Where(ja =>
                    ja.StudentId == applicantId
                    && ja.Status != ApplicationStatus.Pendiente
                    && ja.JobOffer!.IsCvRequired
                )
                .ExecuteUpdateAsync(set => set.SetProperty(ja => ja.IsCVInvalid, true));
            return applicationsToUpdate > 0;
        }
    }
}
