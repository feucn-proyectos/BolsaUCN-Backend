using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Application.DTOs.ReviewDTO.CreateReviewDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;

namespace backend.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz que define la lógica de negocio para la gestión de reseñas.
    /// Proporciona operaciones para crear, actualizar y consultar reseñas entre oferentes y estudiantes.
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Agrega la evaluación del oferente hacia el estudiante.
        /// </summary>
        /// <param name="dto">DTO con la información de la reseña del estudiante.</param>
        /// <param name="currentUserId">ID del usuario autenticado (debe ser el oferente).</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra la reseña.</exception>
        /// <exception cref="UnauthorizedAccessException">Lanzada si el usuario no es el oferente.</exception>
        /// <exception cref="InvalidOperationException">Lanzada si ya completó su evaluación.</exception>
        Task AddStudentReviewAsync(ReviewForStudentDTO dto, int currentUserId);

        /// <summary>
        /// Agrega la evaluación del estudiante hacia el oferente.
        /// </summary>
        /// <param name="dto">DTO con la información de la reseña del oferente.</param>
        /// <param name="currentUserId">ID del usuario autenticado (debe ser el estudiante).</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra la reseña.</exception>
        /// <exception cref="UnauthorizedAccessException">Lanzada si el usuario no es el estudiante.</exception>
        /// <exception cref="InvalidOperationException">Lanzada si ya completó su evaluación.</exception>
        Task AddOfferorReviewAsync(ReviewForOfferorDTO dto, int currentUserId);

        /// <summary>
        /// Método para ejecutar acciones cuando ambas partes completan sus reseñas.
        /// Actualmente no implementado.
        /// </summary>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task BothReviewsCompletedAsync(Review review);

        /// <summary>
        /// Agrega una nueva reseña completa (obsoleto - no implementado).
        /// </summary>
        /// <param name="dto">DTO con la información de la reseña completa.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado.</exception>
        Task AddReviewAsync(ReviewDTO dto);

        /// <summary>
        /// Obtiene todas las reseñas asociadas a un oferente específico.
        /// </summary>
        /// <param name="offerorId">El identificador del oferente.</param>
        /// <returns>Una colección de DTOs de reseñas del oferente.</returns>
        Task<IEnumerable<PublicationAndReviewInfoDTO>> GetReviewsByOfferorAsync(int offerorId);

        /// <summary>
        /// Calcula el promedio de calificaciones de un oferente.
        /// </summary>
        /// <param name="offerorId">El identificador del oferente.</param>
        /// <returns>El promedio de calificaciones, o null si no hay reseñas.</returns>
        Task<double?> GetOfferorAverageRatingAsync(int offerorId);

        /// <summary>
        /// Calcula el promedio de calificaciones de un estudiante.
        /// </summary>
        /// <param name="studentId">El identificador del estudiante.</param>
        /// <returns>El promedio de calificaciones, o null si no hay reseñas.</returns>
        Task<double?> GetStudentAverageRatingAsync(int studentId);

        /// <summary>
        /// Crea una reseña inicial en estado pendiente para una publicación.
        /// Ambas partes deben completar sus evaluaciones posteriormente.
        /// </summary>
        /// <param name="dto">DTO con los identificadores iniciales de la reseña.</param>
        /// <returns>La reseña inicial creada.</returns>
        /// <exception cref="InvalidOperationException">Lanzada si ya existe una reseña para la publicación.</exception>
        Task<Review> CreateInitialReviewAsync(InitialReviewDTO dto);

        /// <summary>
        /// Elimina parcial o completamente una reseña (parte del estudiante, oferente o ambas).
        /// </summary>
        /// <param name="dto">DTO especificando qué partes eliminar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="InvalidOperationException">Lanzada si no se especifica ninguna parte para eliminar.</exception>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra la reseña.</exception>
        Task DeleteReviewPartAsync(DeleteReviewPartDTO dto);

        /// <summary>
        /// Obtiene una reseña específica por su ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ShowReviewDTO> GetReviewAsync(int id);

        /// <summary>
        /// Obtiene todas las reseñas asociadas a un estudiante específico.
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        Task<IEnumerable<PublicationAndReviewInfoDTO>> GetReviewsByStudentAsync(int studentId);

        /// <summary>
        /// Obtiene todas las reseñas del sistema.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PublicationAndReviewInfoDTO>> GetAllReviewsAsync();

        /// <summary>
        /// Obtiene la información de publicaciones asociadas a las reseñas de un usuario.
        /// Identifica automáticamente si el usuario es estudiante u oferente y devuelve las publicaciones correspondientes.
        /// </summary>
        /// <param name="userId">El identificador del usuario (estudiante u oferente).</param>
        /// <returns>Una colección de DTOs de publicaciones y reseñas asociadas al usuario.</returns>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra el usuario o no hay publicaciones.</exception>
        /// <exception cref="InvalidOperationException">Lanzada si el tipo de usuario no puede tener reseñas.</exception>
        Task<IEnumerable<PublicationAndReviewInfoDTO>> GetPublicationInformationAsync(int userId);
        Task UpdateUserRatingAsync(int userId);
        Task<Double?> GetUserAverageRatingAsync(int userId);

        /// <summary>
        /// Cierra las reseñas cuya ventana de revisión terminó, marcándolas como no modificables.
        /// Este método será invocado por un job en segundo plano.
        /// </summary>
        Task CloseExpiredReviewsAsync();

        /// <summary>
        /// Obtiene el número de reseñas pendientes de un usuario.
        /// Una reseña está pendiente si el usuario no ha completado su parte de la evaluación,
        /// la reseña no está cerrada y no está completada por ambas partes.
        /// </summary>
        /// <param name="userId">El identificador del usuario (estudiante u oferente).</param>
        /// <returns>El número de reseñas pendientes del usuario.</returns>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra el usuario.</exception>
        Task<int> GetPendingReviewsCountAsync(User user, string? role = null);

        #region Refactored Methods

        Task<int> CreateInitialReviewsForCompletedOfferAsync(int publicationId);

        Task<string> CreateApplicantReviewForOfferorAsync(
            int reviewId,
            int applicantId,
            ApplicantReviewForOfferorDTO reviewDTO
        );

        Task<string> CreateOfferorReviewForApplicantAsync(
            int reviewId,
            int offerorId,
            OfferorReviewForApplicantDTO reviewDTO
        );
        #endregion
    }
}
