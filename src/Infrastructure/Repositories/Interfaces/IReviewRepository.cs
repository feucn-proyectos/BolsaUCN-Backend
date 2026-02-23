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
        /// Agrega una nueva reseña a la base de datos.
        /// </summary>
        /// <param name="review">La reseña a agregar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task AddAsync(Review review);

        /// <summary>
        /// Obtiene todas las reseñas asociadas a un oferente específico.
        /// </summary>
        /// <param name="offerorId">El identificador del oferente.</param>
        /// <returns>Una colección de reseñas del oferente.</returns>
        Task<IEnumerable<Review>> GetByOfferorIdAsync(int offerorId);

        /// <summary>
        /// Obtiene todas las reseñas asociadas a un estudiante específico.
        /// </summary>
        /// <param name="studentId">El identificador del estudiante.</param>
        /// <returns>Una colección de reseñas del estudiante.</returns>
        Task<IEnumerable<Review>> GetByStudentIdAsync(int studentId);

        /// <summary>
        /// Calcula el promedio de calificaciones recibidas por un oferente.
        /// </summary>
        /// <param name="providerId">El identificador del oferente.</param>
        /// <returns>El promedio de calificaciones, o null si no hay reseñas.</returns>
        Task<double?> GetOfferorAverageRatingAsync(int providerId);

        /// <summary>
        /// Calcula el promedio de calificaciones de un estudiante.
        /// </summary>
        /// <param name="studentId">El identificador del estudiante.</param>
        /// <returns>El promedio de calificaciones, o null si no hay reseñas.</returns>
        Task<double?> GetStudentAverageRatingAsync(int studentId);

        /// <summary>
        /// Obtiene una reseña asociada a una publicación específica.
        /// </summary>
        /// <param name="publicationId">El identificador de la publicación.</param>
        /// <returns>La reseña asociada a la publicación, o null si no existe.</returns>
        Task<Review?> GetByPublicationIdAsync(int publicationId);

        /// <summary>
        /// Obtiene una reseña por su identificador único.
        /// </summary>
        /// <param name="reviewId">El identificador de la reseña.</param>
        /// <returns>La reseña solicitada, o null si no existe.</returns>
        Task<Review?> GetByIdAsync(int reviewId);

        /// <summary>
        /// Actualiza una reseña existente en la base de datos.
        /// </summary>
        /// <param name="review">La reseña con los datos actualizados.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task UpdateAsync(Review review);

        /// <summary>
        /// Obtiene todas las reseñas del sistema.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Review>> GetAllAsync();

        /// <summary>
        /// Obtiene el conteo de reseñas pendientes para un usuario específico, con opción de filtrar por rol.
        /// </summary>
        /// <param name="userId">El identificador del usuario.</param>
        /// <param name="role">El rol del usuario (opcional). Si <c>null</c>, se ignora y se buscan reseñas pendientes para todos los roles.</param>
        /// <returns>El número de reseñas pendientes.</returns>
        Task<int> GetPendingCountOfReviewsByUserIdAsync(int userId, string? role = null);
        Task<IEnumerable<Publication>> GetPublicationInformationAsync(int publicationId);

        #region Refactored Methods
        Task<bool> CreateReviewAsync(NewReview review);
        Task CreateReviewsAsync(IEnumerable<NewReview> reviews);
        Task<NewReview?> GetByIdAsync(int reviewId, ReviewQueryOptions? options = null);
        Task<List<NewReview>> GetReviewsByOfferIdAsync(int offerId);
        Task CalculateUserRating(User user);
        Task<int> GetPendingReviewsCountByOfferIdAsync(int offerId);
        Task<(List<NewReview> reviews, int totalCount)> GetMyReviewsByUserIdAsync(
            MyReviewsSearchParamsDTO searchParams,
            int userId
        );
        Task<NewReview?> GetMyReviewDetailsByIdAsync(int reviewId, int userId);
        Task<(List<NewReview> reviews, int totalCount)> GetAllReviewsForAdminAsync(
            GetReviewsSearchParamsDTO searchParams,
            int adminId
        );
        Task<NewReview?> GetReviewDetailsForAdminByIdAsync(int reviewId);
        Task<bool> UpdateReviewAsync(NewReview review);
        #endregion
    }
}
