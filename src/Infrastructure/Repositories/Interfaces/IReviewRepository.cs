using System;
using backend.src.Application.DTOs.ReviewDTO.AdminDTOs;
using backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO;
using backend.src.Domain.Models;
using backend.src.Domain.Options;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz que define las operaciones de acceso a datos para las reseñas.
    /// Proporciona métodos para crear, consultar y actualizar reseñas en la base de datos.
    /// </summary>
    public interface IReviewRepository
    {
        /// <summary>
        /// Obtiene el conteo de reseñas pendientes para un usuario específico, con opción de filtrar por rol.
        /// </summary>
        /// <param name="userId">El identificador del usuario.</param>
        /// <param name="role">El rol del usuario (opcional). Si <c>null</c>, se ignora y se buscan reseñas pendientes para todos los roles.</param>
        /// <returns>El número de reseñas pendientes.</returns>
        Task<int> GetPendingCountOfReviewsByUserIdAsync(int userId, string? role = null);

        #region Refactored Methods
        Task<bool> CreateReviewAsync(Review review);
        Task<bool> CreateReviewsAsync(IEnumerable<Review> reviews);
        Task<Review?> GetByIdAsync(int reviewId, ReviewQueryOptions? options = null);
        Task<List<Review>> GetReviewsByOfferIdAsync(int offerId);
        Task CalculateUserRating(User user);
        Task<int> GetPendingReviewsCountByOfferIdAsync(int offerId);
        Task<(List<Review> reviews, int totalCount)> GetMyReviewsByUserIdAsync(
            MyReviewsSearchParamsDTO searchParams,
            int userId
        );
        Task<Review?> GetMyReviewDetailsByIdAsync(int reviewId, int userId);
        Task<(List<Review> reviews, int totalCount)> GetAllReviewsForAdminAsync(
            GetReviewsSearchParamsDTO searchParams,
            int adminId
        );
        Task<Review?> GetReviewDetailsForAdminByIdAsync(int reviewId);
        Task<List<Review>> GetAllForAdminAsync();
        Task<List<Review>> GetAllByUserIdAsync(int userId);
        Task<bool> UpdateReviewAsync(Review review);
        #endregion
    }
}
