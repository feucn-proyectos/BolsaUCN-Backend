using backend.src.Application.DTOs.ReviewDTO.AdminDTOs;
using backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Implementación del repositorio de reseñas.
    /// Gestiona las operaciones de persistencia de datos para las reseñas usando Entity Framework Core.
    /// </summary>
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de reseñas.
        /// </summary>
        /// <param name="context">El contexto de base de datos de la aplicación.</param>
        public ReviewRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

        //!REFACTORIZADA
        /// <summary>
        /// Obtiene el conteo de reseñas pendientes para un usuario específico.
        /// </summary>
        /// <param name="userId">El identificador del usuario.</param>
        /// <param name="role">El rol del usuario (opcional). Si el rol es nulo, se consideran ambos roles.</param>
        /// <returns>El número de reseñas pendientes.</returns>
        public async Task<int> GetPendingCountOfReviewsByUserIdAsync(
            int userId,
            string? role = null
        )
        {
            var pendingCountQuery = _context.NewReviews.AsQueryable();
            pendingCountQuery = role switch
            {
                RoleNames.Offeror => pendingCountQuery.Where(r =>
                    r.OfferorId == userId && !r.OfferorReviewCompletedAt.HasValue
                ),
                RoleNames.Applicant => pendingCountQuery.Where(r =>
                    r.ApplicantId == userId && !r.ApplicantReviewCompletedAt.HasValue
                ),
                _ => pendingCountQuery.Where(r =>
                    (r.OfferorId == userId && !r.OfferorReviewCompletedAt.HasValue)
                    || (r.ApplicantId == userId && !r.ApplicantReviewCompletedAt.HasValue)
                ),
            };
            return await pendingCountQuery.CountAsync();
        }

        public async Task<IEnumerable<Publication>> GetPublicationInformationAsync(
            int publicationId
        )
        {
            return await _context.Publications.Where(p => p.Id == publicationId).ToListAsync();
        }

        #region Refactored Methods

        public async Task<bool> CreateReviewAsync(Review review)
        {
            await _context.NewReviews.AddAsync(review);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CreateReviewsAsync(IEnumerable<Review> reviews)
        {
            await _context.NewReviews.AddRangeAsync(reviews);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Review?> GetByIdAsync(int reviewId, ReviewQueryOptions? options = null)
        {
            var query = _context.NewReviews.AsQueryable();

            if (options != null)
            {
                if (options.IncludeApplication)
                    query = query.Include(r => r.Application);
                if (options.IncludeOfferor)
                    query = query.Include(r => r.Offeror);
                if (options.IncludeApplicant)
                    query = query.Include(r => r.Applicant);
                if (!options.TrackChanges)
                    query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        public async Task<List<Review>> GetReviewsByOfferIdAsync(int offerId)
        {
            return await _context
                .NewReviews.Include(r => r.Application)
                .Where(r => r.Application!.JobOfferId == offerId)
                .Include(r => r.Offeror)
                .Include(r => r.Applicant)
                .ToListAsync();
        }

        public async Task CalculateUserRating(User user)
        {
            List<float> ratingsAsOfferor = await _context
                .NewReviews.Where(r =>
                    r.OfferorId == user.Id && r.ApplicantRatingOfOfferor.HasValue
                )
                .Select(r => r.ApplicantRatingOfOfferor!.Value)
                .ToListAsync();

            List<float> ratingsAsApplicant = await _context
                .NewReviews.Where(r =>
                    r.ApplicantId == user.Id && r.OfferorRatingOfApplicant.HasValue
                )
                .Select(r => r.OfferorRatingOfApplicant!.Value)
                .ToListAsync();

            List<float> allRatings = [.. ratingsAsOfferor, .. ratingsAsApplicant];

            if (allRatings.Count > 0)
            {
                user.Rating = allRatings.Average();
            }
        }

        public async Task<int> GetPendingReviewsCountByOfferIdAsync(int offerId)
        {
            return await _context
                .NewReviews.Where(r =>
                    r.Application!.JobOfferId == offerId
                    && (
                        !r.OfferorReviewCompletedAt.HasValue
                        || !r.ApplicantReviewCompletedAt.HasValue
                    )
                )
                .CountAsync();
        }

        public async Task<(List<Review> reviews, int totalCount)> GetMyReviewsByUserIdAsync(
            MyReviewsSearchParamsDTO searchParams,
            int userId
        )
        {
            IQueryable<Review> query = _context
                .NewReviews.Include(r => r.Application)
                .ThenInclude(a => a!.JobOffer)
                .Include(r => r.Offeror)
                .Include(r => r.Applicant)
                .Where(r => r.OfferorId == userId || r.ApplicantId == userId);

            // Filtro por estado de la review
            if (!string.IsNullOrEmpty(searchParams.ReviewStatus))
            {
                query = searchParams.ReviewStatus switch
                {
                    "Pendiente" => query.Where(r =>
                        (r.OfferorId == userId && !r.OfferorRatingOfApplicant.HasValue)
                        || (r.ApplicantId == userId && !r.ApplicantRatingOfOfferor.HasValue)
                    ),
                    "Completada" => query.Where(r =>
                        (r.OfferorRatingOfApplicant.HasValue && r.ApplicantRatingOfOfferor.HasValue)
                        || (r.OfferorId == userId && r.OfferorRatingOfApplicant.HasValue)
                        || (r.ApplicantId == userId && r.ApplicantRatingOfOfferor.HasValue)
                    ),
                    "Cerrada" => query.Where(r => r.ReviewClosedAt.HasValue),
                    _ => query,
                };
            }
            // Busqueda por titulo de publicacion
            if (!string.IsNullOrEmpty(searchParams.PublicationTitle))
            {
                string searchTerm = searchParams.PublicationTitle.ToLower();
                query = query.Where(r =>
                    r.Application!.JobOffer!.Title.ToLower().Contains(searchTerm)
                );
            }
            // Ordenamiento por fecha de creacion de la review
            bool ascending = true; // Orden por defecto.
            if (!string.IsNullOrEmpty(searchParams.SortOrder))
            {
                ascending = searchParams.SortOrder == "asc";
            }
            query = ascending
                ? query.OrderBy(r => r.CreatedAt)
                : query.OrderByDescending(r => r.CreatedAt);

            // Paginacion
            int totalCount = await query.CountAsync();
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            var reviews = await query
                .Skip((searchParams.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
            return (reviews, totalCount);
        }

        public async Task<Review?> GetMyReviewDetailsByIdAsync(int reviewId, int userId)
        {
            var review = await _context
                .NewReviews.Include(r => r.Application)
                .ThenInclude(a => a!.JobOffer)
                .Include(r => r.Offeror)
                .Include(r => r.Applicant)
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.Id == reviewId && (r.OfferorId == userId || r.ApplicantId == userId)
                );

            return review;
        }

        public async Task<(List<Review> reviews, int totalCount)> GetAllReviewsForAdminAsync(
            GetReviewsSearchParamsDTO searchParams,
            int adminId
        )
        {
            IQueryable<Review> query = _context
                .NewReviews.Include(r => r.Application)
                .ThenInclude(a => a!.JobOffer)
                .Include(r => r.Offeror)
                .Include(r => r.Applicant)
                .Where(r => r.OfferorId != adminId);

            // Filtro por estado de la review
            if (!string.IsNullOrEmpty(searchParams.FilterByReviewStatus))
            {
                query = searchParams.FilterByReviewStatus switch
                {
                    "Pendiente" => query.Where(r =>
                        !r.OfferorRatingOfApplicant.HasValue || !r.ApplicantRatingOfOfferor.HasValue
                    ),
                    "OferenteEvaluoEstudiante" => query.Where(r =>
                        r.OfferorRatingOfApplicant.HasValue && !r.ApplicantRatingOfOfferor.HasValue
                    ),
                    "EstudianteEvaluoOferente" => query.Where(r =>
                        r.ApplicantRatingOfOfferor.HasValue && !r.OfferorRatingOfApplicant.HasValue
                    ),
                    "Completada" => query.Where(r =>
                        r.OfferorRatingOfApplicant.HasValue && r.ApplicantRatingOfOfferor.HasValue
                    ),
                    "Cerrada" => query.Where(r => r.ReviewClosedAt.HasValue),
                    _ => query,
                };
            }
            // Busqueda por titulo de publicacion
            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                string searchTerm = searchParams.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.Application!.JobOffer!.Title.ToLower().Contains(searchTerm)
                );
            }
            // Ordenamiento por fecha de creacion de la review
            bool ascending = true; // Orden por defecto.
            if (!string.IsNullOrEmpty(searchParams.SortOrder))
            {
                ascending = searchParams.SortOrder == "asc";
            }
            query = searchParams.SortBy switch
            {
                "JobOfferTitle" => ascending
                    ? query.OrderBy(r => r.Application!.JobOffer!.Title)
                    : query.OrderByDescending(r => r.Application!.JobOffer!.Title),
                "OpenUntil" => ascending
                    ? query.OrderBy(r => r.Application!.JobOffer!.ReviewDeadline)
                    : query.OrderByDescending(r => r.Application!.JobOffer!.ReviewDeadline),
                _ => query.OrderByDescending(r => r.CreatedAt),
            };

            // Paginacion
            int totalCount = await query.CountAsync();
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            var reviews = await query
                .Skip((searchParams.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
            return (reviews, totalCount);
        }

        public async Task<Review?> GetReviewDetailsForAdminByIdAsync(int reviewId)
        {
            var review = await _context
                .NewReviews.Include(r => r.Application)
                .ThenInclude(a => a!.JobOffer)
                .Include(r => r.Offeror)
                .Include(r => r.Applicant)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            return review;
        }

        public async Task<List<Review>> GetAllForAdminAsync()
        {
            return await _context
                .NewReviews.Include(r => r.Application)
                .ThenInclude(a => a!.JobOffer)
                .Include(r => r.Offeror)
                .Include(r => r.Applicant)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Review>> GetAllByUserIdAsync(int userId)
        {
            return await _context
                .NewReviews.Include(r => r.Application)
                .ThenInclude(a => a!.JobOffer)
                .Include(r => r.Offeror)
                .Include(r => r.Applicant)
                .Where(r =>
                    (r.OfferorId == userId && r.ApplicantRatingOfOfferor.HasValue)
                    || (r.ApplicantId == userId && r.OfferorRatingOfApplicant.HasValue)
                )
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateReviewAsync(Review review)
        {
            _context.NewReviews.Update(review);
            return await _context.SaveChangesAsync() > 0;
        }

        #endregion
    }
}
