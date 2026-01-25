using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Application.Mappers;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;
using backend.src.Infrastructure.Repositories.Interfaces;
using Serilog;

// Este codigo funciona en base a sueños y esperanzas, y mucho Claude Sonnet 4.5
// lo lamento para la pobre alma que tenga que mantener esto en el futuro.
namespace backend.src.Application.Services.Implements
{
    /// <summary>
    /// Implementación del servicio de reseñas.
    /// Gestiona la lógica de negocio para las operaciones de reseñas bidireccionales
    /// entre oferentes y estudiantes, incluyendo validaciones y permisos.
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IAdminNotificationRepository _adminNotificationRepository;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de reseñas.
        /// </summary>
        /// <param name="repository">El repositorio de reseñas para acceso a datos.</param>
        /// <param name="userRepository">El repositorio de usuarios para validación.</param>
        public ReviewService(
            IReviewRepository repository,
            IUserRepository userRepository,
            IAdminNotificationRepository adminNotificationRepository,
            IEmailService emailService
        )
        {
            _repository = repository;
            _userRepository = userRepository;
            _adminNotificationRepository = adminNotificationRepository;
            _emailService = emailService;
        }

        #region Obtener Reviews y Ratings
        /// <summary>
        /// Agrega una nueva reseña completa (obsoleto - no implementado).
        /// </summary>
        /// <param name="dto">DTO con la información de la reseña completa.</param>
        /// <exception cref="NotImplementedException">Este método no está implementado.</exception>
        public async Task AddReviewAsync(ReviewDTO dto)
        {
            // var review = ReviewMapper.ToEntity(dto);
            // await _repository.AddAsync(review);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtiene todas las reseñas asociadas a un oferente específico.
        /// </summary>
        /// <param name="offerorId">El identificador del oferente.</param>
        /// <returns>Una colección de DTOs de reseñas del oferente.</returns>
        public async Task<IEnumerable<PublicationAndReviewInfoDTO>> GetReviewsByOfferorAsync(
            int offerorId
        )
        {
            var reviews = await _repository.GetByOfferorIdAsync(offerorId);
            if (reviews == null || !reviews.Any())
                throw new KeyNotFoundException(
                    $"No se encontraron reseñas para el oferente con ID {offerorId}."
                );

            var user = await _userRepository.GetUserByIdAsync(offerorId);

            var result = new List<PublicationAndReviewInfoDTO>();
            foreach (var review in reviews)
            {
                var publications = await _repository.GetPublicationInformationAsync(
                    review.PublicationId
                );
                if (publications != null)
                {
                    foreach (var publication in publications)
                    {
                        result.Add(
                            ReviewMapper.MapToPublicationAndReviewInfoDTO(
                                review,
                                publication,
                                user.UserType
                            )
                        );
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Calcula el promedio de calificaciones recibidas por un oferente.
        /// </summary>
        /// <param name="offerorId">El identificador del oferente.</param>
        /// <returns>El promedio de calificaciones, o null si no hay reseñas.</returns>
        public async Task<double?> GetOfferorAverageRatingAsync(int offerorId)
        {
            return await _repository.GetOfferorAverageRatingAsync(offerorId);
        }

        public async Task<double?> GetStudentAverageRatingAsync(int studentId)
        {
            return await _repository.GetStudentAverageRatingAsync(studentId);
        }

        public async Task<ShowReviewDTO> GetReviewAsync(int id)
        {
            var review =
                await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró una review con ID {id}.");
            return ReviewMapper.ShowReviewDTO(review);
        }

        public async Task<IEnumerable<PublicationAndReviewInfoDTO>> GetReviewsByStudentAsync(
            int studentId
        )
        {
            var reviews = await _repository.GetByStudentIdAsync(studentId);
            if (reviews == null || !reviews.Any())
                throw new KeyNotFoundException(
                    $"No se encontraron reseñas para el estudiante con ID {studentId}."
                );

            var result = new List<PublicationAndReviewInfoDTO>();
            foreach (var review in reviews)
            {
                var publications = await _repository.GetPublicationInformationAsync(
                    review.PublicationId
                );
                if (publications != null)
                {
                    foreach (var publication in publications)
                    {
                        result.Add(
                            ReviewMapper.MapToPublicationAndReviewInfoDTO(
                                review,
                                publication,
                                UserType.Estudiante
                            )
                        );
                    }
                }
            }
            return result;
        }

        public async Task<IEnumerable<PublicationAndReviewInfoDTO>> GetAllReviewsAsync()
        {
            var reviews = await _repository.GetAllAsync();
            var result = new List<PublicationAndReviewInfoDTO>();
            foreach (var review in reviews)
            {
                var publications = await _repository.GetPublicationInformationAsync(
                    review.PublicationId
                );
                if (publications != null)
                {
                    foreach (var publication in publications)
                    {
                        result.Add(
                            ReviewMapper.MapToPublicationAndReviewInfoDTO(
                                review,
                                publication,
                                UserType.Administrador
                            )
                        );
                    }
                }
            }
            return result;
        }
        #endregion
        #region Agregar Reviews
        /// <summary>
        /// Agrega la evaluación del oferente hacia el estudiante.
        /// Valida que el usuario autenticado sea el oferente y que no haya completado previamente su evaluación.
        /// Si el estudiante ya completó su parte, marca la reseña como completada.
        /// </summary>
        /// <param name="dto">DTO con la calificación y comentarios para el estudiante.</param>
        /// <param name="currentUserId">ID del usuario autenticado (debe ser el oferente).</param>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra una reseña para el ID de publicación.</exception>
        /// <exception cref="UnauthorizedAccessException">Lanzada si el usuario no es el oferente de esta publicación.</exception>
        /// <exception cref="InvalidOperationException">Lanzada si el oferente ya completó su evaluación.</exception>
        public async Task AddStudentReviewAsync(ReviewForStudentDTO dto, int currentUserId)
        {
            var review =
                await _repository.GetByIdAsync(dto.ReviewId)
                ?? throw new KeyNotFoundException(
                    "No se ha encontrado una reseña para el ID de publicación dado."
                );

            // Validar que el usuario actual sea el OFERENTE (quien califica al estudiante)
            if (review.OfferorId != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "Solo el oferente de esta publicación puede dejar una review hacia el estudiante."
                );
            }
            // Validar que el oferente no haya completado ya su review hacia el estudiante
            if (review.IsReviewForStudentCompleted)
            {
                throw new InvalidOperationException(
                    "Ya has completado tu review para este estudiante."
                );
            }
            // Validar que la review no esté cerrada por el proceso automático
            if (review.IsClosed)
            {
                throw new InvalidOperationException(
                    "No se puede modificar esta review porque ha sido cerrada."
                );
            }
            if (review.HasReviewForStudentBeenDeleted)
            {
                throw new InvalidOperationException(
                    "No se puede agregar la Review hacia el estudiante, ya que esta ya ha sido eliminada."
                );
            }
            ReviewMapper.StudentUpdateReview(dto, review);
            if (review.IsCompleted)
                await BothReviewsCompletedAsync(review);
            await _repository.UpdateAsync(review);
            Log.Information(
                "Offeror {OfferorId} added review for student in review {ReviewId}",
                currentUserId,
                dto.ReviewId
            );

            if (review.RatingForStudent.HasValue && review.RatingForStudent <= 3)
            {
                await _emailService.SendLowRatingReviewAlertAsync(
                    new ReviewDTO
                    {
                        IdReview = review.Id,
                        RatingForStudent = review.RatingForStudent,
                        CommentForStudent = review.CommentForStudent,
                        IdStudent = review.StudentId,
                        IdOfferor = review.OfferorId,
                        IdPublication = review.PublicationId,
                        AtTime = review.ReviewChecklistValues.AtTime,
                        GoodPresentation = review.ReviewChecklistValues.GoodPresentation,
                        StudentHasRespectOfferor = review
                            .ReviewChecklistValues
                            .StudentHasRespectOfferor,
                    }
                );
            }
        }

        /// <summary>
        /// Agrega la evaluación del estudiante hacia el oferente.
        /// Valida que el usuario autenticado sea el estudiante y que no haya completado previamente su evaluación.
        /// Si el oferente ya completó su parte, marca la reseña como completada.
        /// </summary>
        /// <param name="dto">DTO con la calificación y comentarios para el oferente.</param>
        /// <param name="currentUserId">ID del usuario autenticado (debe ser el estudiante).</param>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra una reseña para el ID de publicación.</exception>
        /// <exception cref="UnauthorizedAccessException">Lanzada si el usuario no es el estudiante de esta publicación.</exception>
        /// <exception cref="InvalidOperationException">Lanzada si el estudiante ya completó su evaluación.</exception>
        public async Task AddOfferorReviewAsync(ReviewForOfferorDTO dto, int currentUserId)
        {
            var review =
                await _repository.GetByIdAsync(dto.ReviewId)
                ?? throw new KeyNotFoundException(
                    "No se ha encontrado una reseña para el ID de publicación dado."
                );
            // Validar que el usuario actual sea el ESTUDIANTE (quien califica al oferente)
            if (review.StudentId != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "Solo el estudiante de esta publicación puede dejar una review hacia el oferente."
                );
            }
            // Validar que el estudiante no haya completado ya su review hacia el oferente
            if (review.IsReviewForOfferorCompleted)
            {
                throw new InvalidOperationException(
                    "Ya has completado tu review para este oferente."
                );
            }
            // Validar que la review no esté cerrada por el proceso automático
            if (review.IsClosed)
            {
                throw new InvalidOperationException(
                    "No se puede modificar esta review porque ha sido cerrada."
                );
            }
            if (review.HasReviewForOfferorBeenDeleted)
            {
                throw new InvalidOperationException(
                    "No se puede agregar la Review hacia el oferente, ya que esta ya ha sido eliminada."
                );
            }
            ReviewMapper.OfferorUpdateReview(dto, review);
            if (review.IsCompleted)
                await BothReviewsCompletedAsync(review);
            await _repository.UpdateAsync(review);
            Log.Information(
                "Student {StudentId} added review for offeror in Review {ReviewId}",
                currentUserId,
                dto.ReviewId
            );

            if (review.RatingForOfferor.HasValue && review.RatingForOfferor <= 3)
            {
                await _emailService.SendLowRatingReviewAlertAsync(
                    new ReviewDTO
                    {
                        IdReview = review.Id,
                        RatingForOfferor = review.RatingForOfferor,
                        CommentForOfferor = review.CommentForOfferor,
                        IdStudent = review.StudentId,
                        IdOfferor = review.OfferorId,
                        IdPublication = review.PublicationId,
                        AtTime = review.ReviewChecklistValues.AtTime,
                        GoodPresentation = review.ReviewChecklistValues.GoodPresentation,
                        StudentHasRespectOfferor = review
                            .ReviewChecklistValues
                            .StudentHasRespectOfferor,
                    }
                );
            }
        }

        /// <summary>
        /// Método para ejecutar acciones cuando ambas partes completan sus reseñas.
        /// Actualiza los ratings promedio tanto del oferente como del estudiante.
        /// </summary>
        /// <param name="review">La reseña que ha sido completada por ambas partes.</param>
        public async Task BothReviewsCompletedAsync(Review review)
        {
            review.IsClosed = true;
            // Actualizar el Rating del oferente y del estudiante
            await UpdateUserRatingAsync(review.OfferorId);
            await UpdateUserRatingAsync(review.StudentId);
            Log.Information(
                "Se actualizan rating para el oferente: {OfferorId} y el estudiante {StudentId} despues de completar la Review",
                review.OfferorId,
                review.StudentId
            );
        }

        /// <summary>
        /// Crea una reseña inicial en estado pendiente para una publicación.
        /// La ventana de revisión se establece automáticamente en 14 días.
        /// Ambas partes deben completar sus evaluaciones posteriormente.
        /// </summary>
        /// <param name="dto">DTO con los identificadores del estudiante, oferente y publicación.</param>
        /// <returns>La reseña inicial creada con estado pendiente.</returns>
        /// <exception cref="InvalidOperationException">Lanzada si ya existe una reseña para la publicación especificada.</exception>
        /// <exception cref="ArgumentException">Lanzada si el estudiante u oferente no existen.</exception>
        public async Task<Review> CreateInitialReviewAsync(InitialReviewDTO dto)
        {
            // Validar que existan el estudiante y el oferente
            var student =
                await _userRepository.GetUserByIdAsync(dto.StudentId)
                ?? throw new ArgumentException($"El estudiante con ID {dto.StudentId} no existe.");
            var offeror =
                await _userRepository.GetUserByIdAsync(dto.OfferorId)
                ?? throw new ArgumentException($"El oferente con ID {dto.OfferorId} no existe.");
            // Validar que no exista ya una review para esta publicación
            var existingReview = await _repository.GetByPublicationIdAsync(dto.PublicationId);
            if (existingReview != null)
            {
                throw new InvalidOperationException(
                    $"Ya existe una review para la publicación con ID {dto.PublicationId}."
                );
            }
            var review = ReviewMapper.CreateInitialReviewAsync(dto, student, offeror);
            await _repository.AddAsync(review);
            return review;
        }
        #endregion
        #region Eliminar Reviews
        /// <summary>
        /// Elimina parcial o completamente una reseña.
        /// Permite eliminar la parte del estudiante, del oferente, o ambas.
        /// Si se eliminan ambas partes, la reseña se marca como no completada.
        /// </summary>
        /// <param name="dto">DTO especificando qué partes eliminar (estudiante y/o oferente).</param>
        /// <exception cref="InvalidOperationException">Lanzada si no se especifica ninguna parte para eliminar.</exception>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra la reseña con el ID especificado.</exception>
        public async Task DeleteReviewPartAsync(DeleteReviewPartDTO dto)
        {
            // Validar que se solicite eliminar al menos una parte
            if (!dto.DeleteReviewForOfferor && !dto.DeleteReviewForStudent)
            {
                throw new InvalidOperationException(
                    "Debe especificar al menos una parte de la review para eliminar."
                );
            }
            // Obtener la review
            var review =
                await _repository.GetByIdAsync(dto.ReviewId)
                ?? throw new KeyNotFoundException(
                    $"No se encontró una review con ID {dto.ReviewId}."
                );
            // Si la review fue cerrada automáticamente, no se permiten cambios
            if (review.IsClosed)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar parte de esta review porque ha sido cerrada."
                );
            }
            // Eliminar la parte del estudiante si se solicita
            if (dto.DeleteReviewForStudent)
                ReviewMapper.DeleteReviewForStudent(review);
            // Eliminar la parte del oferente si se solicita
            if (dto.DeleteReviewForOfferor)
                ReviewMapper.DeleteReviewForOfferor(review);
            await _repository.UpdateAsync(review);
        }
        #endregion
        #region Otras operaciones

        public async Task<IEnumerable<PublicationAndReviewInfoDTO>> GetPublicationInformationAsync(
            int userId
        )
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            IEnumerable<Review> reviews;
            if (user.UserType == UserType.Estudiante)
            {
                reviews = await _repository.GetByStudentIdAsync(userId);
                if (reviews == null || !reviews.Any())
                    throw new KeyNotFoundException(
                        $"No se encontraron reseñas para el estudiante con ID {userId}."
                    );
            }
            else if (user.UserType == UserType.Empresa || user.UserType == UserType.Particular)
            {
                reviews = await _repository.GetByOfferorIdAsync(userId);
                if (reviews == null || !reviews.Any())
                    throw new KeyNotFoundException(
                        $"No se encontraron reseñas para el oferente con ID {userId}."
                    );
            }
            else
            {
                throw new InvalidOperationException(
                    $"El tipo de usuario {user.UserType} no puede tener reseñas."
                );
            }
            var result = new List<PublicationAndReviewInfoDTO>();
            foreach (var review in reviews)
            {
                var publications = await _repository.GetPublicationInformationAsync(review.Id);
                if (publications != null)
                {
                    foreach (var publication in publications)
                    {
                        result.Add(
                            ReviewMapper.MapToPublicationAndReviewInfoDTO(
                                review,
                                publication,
                                user.UserType
                            )
                        );
                    }
                }
            }
            if (result.Count == 0)
                throw new KeyNotFoundException(
                    $"No se encontró información de publicación para el usuario con ID {userId}."
                );
            return result;
        }

        public async Task UpdateUserRatingAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(
                userId,
                new UserQueryOptions { TrackChanges = true }
            );
            double? averageRating = null;
            // Determinar el tipo de usuario y obtener su calificación promedio
            if (user.UserType == UserType.Estudiante)
            {
                averageRating = await _repository.GetStudentAverageRatingAsync(userId);
            }
            else if (user.UserType == UserType.Empresa || user.UserType == UserType.Particular)
            {
                averageRating = await _repository.GetOfferorAverageRatingAsync(userId);
            }
            else
            {
                throw new InvalidOperationException(
                    $"El tipo de usuario {user.UserType} no puede tener calificaciones."
                );
            }
            // Actualizar el rating del usuario (usar 0.0 si no hay calificaciones)
            user.Rating = averageRating ?? 0.0;
            await _userRepository.UpdateAsync(user);
            Log.Information(
                "Se actualizo el rating del usuario: {UserId} a: {Rating}",
                userId,
                user.Rating
            );
        }

        public async Task<double?> GetUserAverageRatingAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {userId}.");
            }
            if (user.UserType == UserType.Estudiante)
            {
                return await GetStudentAverageRatingAsync(userId);
            }
            else if (user.UserType == UserType.Empresa || user.UserType == UserType.Particular)
            {
                return await GetOfferorAverageRatingAsync(userId);
            }
            else
            {
                throw new InvalidOperationException(
                    $"El tipo de usuario {user.UserType} no puede tener calificaciones."
                );
            }
        }

        /// <summary>
        /// Obtiene el número de reseñas pendientes de un usuario.
        /// Una reseña está pendiente si el usuario no ha completado su parte,
        /// la reseña no está cerrada y no está completada por ambas partes.
        /// </summary>
        /// <param name="userId">El identificador del usuario (estudiante u oferente).</param>
        /// <returns>El número de reseñas pendientes del usuario.</returns>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra el usuario.</exception>
        public async Task<int> GetPendingReviewsCountAsync(User user, string? role = null)
        {
            // Corrobora que el usuario tenga el rol por el cual se esta filtrando
            if (role != null)
            {
                var hasRoleResult = await _userRepository.CheckRoleAsync(user.Id, role);
                if (!hasRoleResult)
                {
                    Log.Error("El usuario con ID {UserId} no tiene el rol {Role}", user.Id, role);
                    throw new UnauthorizedAccessException(
                        $"El usuario con ID {user.Id} no tiene el rol {role}."
                    );
                }
            }
            // Contar el numero de reseñas pendientes por medio de filtrado por ID
            int pendingCount = await _repository.GetPendingCountOfReviewsByUserIdAsync(
                user.Id,
                role
            );
            Log.Information(
                "Usuario {UserType} con ID {UserId} tiene {PendingCount} reseñas pendientes",
                user.UserType,
                user.Id,
                pendingCount
            );
            return pendingCount;
        }

        #endregion
        #region Hangfire
        /// <summary>
        /// Cierra las reseñas cuya ventana de revisión terminó y las marca como no modificables.
        /// </summary>
        /// Las clientas no quieren que se cierren automaticamente, a si que este metodo queda obsoleto
        public async Task CloseExpiredReviewsAsync()
        {
            return;
            // var now = DateTime.UtcNow;
            // var expiredReviews = await _repository.GetExpiredReviewsAsync(now);
            // if (expiredReviews == null || !expiredReviews.Any())
            // {
            //     Log.Information("No hay reviews vencidas para cerrar a las {Now}", now);
            //     return;
            // }
            // foreach (var review in expiredReviews)
            // {
            //     review.IsClosed = true;
            //     await _repository.UpdateAsync(review);
            //     Log.Information("Review {ReviewId} cerrada automáticamente por vencimiento.", review.Id);
            // }
            // Log.Information("Cierre automático de reviews vencidas completado. Total cerrado: {Count}", expiredReviews.Count());
        }
        #endregion
    }
}
