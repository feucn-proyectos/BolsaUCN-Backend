using backend.src.Application.DTOs.ReviewDTO.AdminDTOs;
using backend.src.Application.DTOs.ReviewDTO.CreateReviewDTOs;
using backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
using backend.src.Infrastructure.Repositories.Interfaces;
using Hangfire;
using Mapster;
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
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPublicationRepository _publicationRepository;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly int _daysUntilReviewAutoClose;
        private readonly int _defaultPageSize;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de reseñas.
        /// </summary>
        /// <param name="repository">El repositorio de reseñas para acceso a datos.</param>
        /// <param name="userRepository">El repositorio de usuarios para validación.</param>
        public ReviewService(
            IReviewRepository repository,
            IUserRepository userRepository,
            IPublicationRepository publicationRepository,
            INotificationService notificationService,
            IConfiguration configuration
        )
        {
            _reviewRepository = repository;
            _userRepository = userRepository;
            _publicationRepository = publicationRepository;
            _notificationService = notificationService;
            _configuration = configuration;
            _daysUntilReviewAutoClose = _configuration.GetValue<int>(
                "JobsConfiguration:DaysUntilReviewAutoClose"
            );
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

        #region Agregar Reviews
        public async Task<int> CreateInitialReviewsForCompletedOfferAsync(int offerId)
        {
            Log.Information("Inicializando reviews para la oferta con ID {OfferId}", offerId);
            // Validacion de oferta
            Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                offerId,
                new PublicationQueryOptions { IncludeUser = true, IncludeApplications = true }
            );
            if (offer == null)
            {
                Log.Error(
                    "No se encontró la oferta con ID {OfferId} para inicializar la review.",
                    offerId
                );
                throw new KeyNotFoundException($"No se encontró la oferta con ID {offerId}.");
            }
            // Validacion de postulantes aceptados
            var acceptedApplications = offer.Applications.Where(a =>
                a.Status == ApplicationStatus.Aceptada
            );
            if (!acceptedApplications.Any())
            {
                Log.Error(
                    "No se puede inicializar la review para la oferta con ID {OfferId} porque no tiene postulantes aceptados.",
                    offerId
                );
                throw new InvalidOperationException(
                    $"No se puede inicializar la review para la oferta con ID {offerId} porque no tiene postulantes aceptados."
                );
            }
            Log.Information(
                "Se encontraron {AcceptedCount} postulantes aceptados para la oferta con ID {OfferId}. Se procederá a crear las reviews iniciales.",
                acceptedApplications.Count(),
                offerId
            );
            // Crear una review inicial para cada postulante aceptado y se asocia a la postulación correspondiente
            // Si bien, lo ideal seria juntar todas las reseñares en una colleccion y hacer una sola llamada al repositorio para crear todas las reseñas de una,
            // para facilitar las consultas posteriores, se opta por crear cada reseña individualmente y asociarla a su postulación correspondiente, de esta forma no es necesario hacer consultas adicionales para relacionar las reseñas con las postulaciones.
            List<Review> createdReviews = acceptedApplications
                .Select(application => new Review
                {
                    ApplicationId = application.Id,
                    OfferorId = offer.UserId,
                    ApplicantId = application.StudentId,
                })
                .ToList();
            bool allCreatedResult = await _reviewRepository.CreateReviewsAsync(createdReviews);
            if (!allCreatedResult)
            {
                Log.Error(
                    "No se pudieron crear las reviews iniciales para la oferta con ID {OfferId}",
                    offerId
                );
                throw new InvalidOperationException(
                    $"No se pudieron crear las reviews iniciales para la oferta con ID {offerId}."
                );
            }

            Log.Information(
                "Reviews iniciales creadas para la oferta con ID {OfferId}. Se programará el cierre automático de las reviews después de {Days} días.",
                offerId,
                _daysUntilReviewAutoClose
            );
            return acceptedApplications.Count();
        }

        public async Task<string> CreateApplicantReviewForOfferorAsync(
            int reviewId,
            int applicantId,
            ApplicantReviewForOfferorDTO reviewDTO
        )
        {
            // Validar al usuario
            User? user = await _userRepository.GetByIdAsync(applicantId);
            if (user == null)
            {
                Log.Error(
                    "No se encontró el usuario con ID {ApplicantId} para crear la review hacia el oferente.",
                    applicantId
                );
                throw new KeyNotFoundException($"No se encontró el usuario con ID {applicantId}.");
            }
            bool isApplicant = await _userRepository.CheckRoleAsync(
                applicantId,
                RoleNames.Applicant
            );
            if (!isApplicant)
            {
                Log.Error(
                    "El usuario con ID {ApplicantId} no tiene el rol de postulante para crear la review hacia el oferente.",
                    applicantId
                );
                throw new UnauthorizedAccessException(
                    $"El usuario con ID {applicantId} no tiene el rol de postulante para crear la review hacia el oferente."
                );
            }

            // Validar la review
            Review? review = await _reviewRepository.GetByIdAsync(
                reviewId,
                new ReviewQueryOptions
                {
                    TrackChanges = true,
                    IncludeApplicant = true,
                    IncludeOfferor = true,
                    IncludeApplication = true,
                }
            );
            if (review == null)
            {
                Log.Error(
                    "No se encontró la review con ID {ReviewId} para crear la review hacia el oferente.",
                    reviewId
                );
                throw new KeyNotFoundException($"No se encontró la review con ID {reviewId}.");
            }
            if (review.HasApplicantEvaluatedOfferor)
            {
                Log.Error(
                    "La review con ID {ReviewId} ya ha sido completada para el oferente. No se puede crear una nueva review, o editar la review hacia el oferente.",
                    reviewId
                );
                throw new InvalidOperationException(
                    $"La review con ID {reviewId} ya ha sido completada para el oferente."
                );
            }
            if (review.IsClosed && !review.IsCompleted)
            {
                Log.Error(
                    "La review con ID {ReviewId} ha sido cerrada automáticamente por vencimiento. No se puede crear una nueva review, o editar la review hacia el oferente.",
                    reviewId
                );
                throw new InvalidOperationException(
                    $"La review con ID {reviewId} ha sido cerrada automáticamente por vencimiento."
                );
            }
            if (review.ApplicantId != applicantId)
            {
                Log.Error(
                    "El usuario con ID {ApplicantId} no es el postulante asociado a la review con ID {ReviewId}.",
                    applicantId,
                    reviewId
                );
                throw new UnauthorizedAccessException(
                    $"El usuario con ID {applicantId} no es el postulante asociado a la review con ID {reviewId}."
                );
            }
            // Mapeo de la información de la review hacia el oferente
            reviewDTO.Adapt(review);
            bool updated = await _reviewRepository.UpdateReviewAsync(review);
            if (!updated)
            {
                Log.Error("No se pudo actualizar la review con ID {ReviewId}", review.Id);
                throw new InvalidOperationException("No se pudo actualizar la review");
            }
            Log.Information(
                "Review con ID {ReviewId} actualizada por el postulante con ID {ApplicantId} para el oferente con ID {OfferorId}",
                review.Id,
                applicantId,
                review.OfferorId
            );
            // Si ambas partes completaron sus reseñas se revisan los siguientes efectos secundarios
            if (review.IsCompleted)
            {
                // Se calcula el nuevo puntaje del oferente y postulante con base en la nueva review creada, y se actualizan sus puntajes promedio en la base de datos.
                await _reviewRepository.CalculateUserRating(review.Applicant!);
                await _reviewRepository.CalculateUserRating(review.Offeror!);

                // Se encola una notificacion para el admin informando que una review ha sido completada, para que pueda revisar la review y ocultar información si es necesario.
                await _notificationService.CreateAdminNotificationAsync(
                    AdminNotificationType.CalificacionCompletada,
                    $"La review con ID {review.Id} ha sido completada. Requiere revisión."
                );

                // Se verifica si la oferta a la que esta asociada la reseña tiene reseñas pendientes fuera de esta, si no tiene reseñas pendientes, se procede a finalizar la oferta.
                int offerId = review.Application!.JobOfferId;
                int pendingReviewsCount =
                    await _reviewRepository.GetPendingReviewsCountByOfferIdAsync(offerId);
                if (pendingReviewsCount == 0)
                {
                    Log.Information(
                        "La oferta con ID {OfferId} no tiene reseñas pendientes después de completar la review con ID {ReviewId}. Se procederá a finalizar la publicación.",
                        offerId,
                        review.Id
                    );
                    Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                        offerId
                    );
                    BackgroundJob.Reschedule(
                        offer!.FinalizeAndCloseReviewsJobId,
                        DateTimeOffset.UtcNow
                    );
                }
            }

            return "Review creada exitosamente para el oferente.";
        }

        public async Task<string> CreateOfferorReviewForApplicantAsync(
            int reviewId,
            int offerorId,
            OfferorReviewForApplicantDTO reviewDTO
        )
        {
            // Validar al usuario
            User? user = await _userRepository.GetByIdAsync(offerorId);
            if (user == null)
            {
                Log.Error(
                    "No se encontró el usuario con ID {OfferorId} para crear la review hacia el postulante.",
                    offerorId
                );
                throw new KeyNotFoundException($"No se encontró el usuario con ID {offerorId}.");
            }
            bool isOfferor = await _userRepository.CheckRoleAsync(offerorId, RoleNames.Offeror);
            if (!isOfferor)
            {
                Log.Error(
                    "El usuario con ID {OfferorId} no tiene el rol de oferente para crear la review hacia el postulante.",
                    offerorId
                );
                throw new UnauthorizedAccessException(
                    $"El usuario con ID {offerorId} no tiene el rol de oferente para crear la review hacia el postulante."
                );
            }

            // Validar la review
            Review? review = await _reviewRepository.GetByIdAsync(
                reviewId,
                new ReviewQueryOptions
                {
                    TrackChanges = true,
                    IncludeApplicant = true,
                    IncludeOfferor = true,
                    IncludeApplication = true,
                }
            );
            if (review == null)
            {
                Log.Error(
                    "No se encontró la review con ID {ReviewId} para crear la review hacia el oferente.",
                    reviewId
                );
                throw new KeyNotFoundException($"No se encontró la review con ID {reviewId}.");
            }
            if (review.OfferorId != offerorId)
            {
                Log.Error(
                    "El usuario con ID {OfferorId} no es el oferente asociado a la review con ID {ReviewId}.",
                    offerorId,
                    reviewId
                );
                throw new UnauthorizedAccessException(
                    $"El usuario con ID {offerorId} no es el oferente asociado a la review con ID {reviewId}."
                );
            }
            if (review.HasOfferorEvaluatedApplicant)
            {
                Log.Error(
                    "La review con ID {ReviewId} ya ha sido completada para el postulante. No se puede crear una nueva review, o editar la review hacia el oferente.",
                    reviewId
                );
                throw new InvalidOperationException(
                    $"La review con ID {reviewId} ya ha sido completada para el postulante."
                );
            }
            // Mapeo de la información de la review hacia el oferente
            reviewDTO.Adapt(review);
            bool updated = await _reviewRepository.UpdateReviewAsync(review);
            if (!updated)
            {
                Log.Error("No se pudo actualizar la review con ID {ReviewId}", review.Id);
                throw new InvalidOperationException("No se pudo actualizar la review");
            }
            Log.Information(
                "Review con ID {ReviewId} actualizada por el oferente con ID {OfferorId} para el postulante con ID {ApplicantId}",
                review.Id,
                offerorId,
                review.ApplicantId
            );
            // Verifica si la oferta a la que esta asociada la reseña tiene reseñas pendientes fuera de esta
            if (review.IsCompleted)
            {
                // Se calcula el nuevo puntaje del oferente y postulante con base en la nueva review creada, y se actualizan sus puntajes promedio en la base de datos.
                await _reviewRepository.CalculateUserRating(review.Applicant!);
                await _reviewRepository.CalculateUserRating(review.Offeror!);

                // Se encola una notificacion para el admin informando que una review ha sido completada, para que pueda revisar la review y ocultar información si es necesario.
                await _notificationService.CreateAdminNotificationAsync(
                    AdminNotificationType.CalificacionCompletada,
                    $"La review con ID {review.Id} ha sido completada. Requiere revisión."
                );

                int offerId = review.Application!.JobOfferId;
                int pendingReviewsCount =
                    await _reviewRepository.GetPendingReviewsCountByOfferIdAsync(offerId);
                if (pendingReviewsCount == 0)
                {
                    Log.Information(
                        "La oferta con ID {OfferId} no tiene reseñas pendientes después de completar la review con ID {ReviewId}. Se procederá a finalizar la publicación.",
                        offerId,
                        review.Id
                    );
                    Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                        offerId
                    );
                    BackgroundJob.Reschedule(
                        offer!.FinalizeAndCloseReviewsJobId,
                        DateTimeOffset.UtcNow
                    );
                }
            }
            return "Review creada exitosamente para el postulante.";
        }
        #endregion

        #region Get Reviews with Pagination
        public async Task<MyReviewsDTO> GetMyReviewsAsync(
            MyReviewsSearchParamsDTO searchParams,
            int userId
        )
        {
            // Validar al usuario
            bool userExists = await _userRepository.ExistsByIdAsync(userId);
            if (!userExists)
            {
                Log.Error(
                    "No se encontró el usuario con ID {UserId} para obtener sus reviews.",
                    userId
                );
                throw new KeyNotFoundException($"No se encontró el usuario con ID {userId}.");
            }
            // Obtener las reviews del usuario
            (List<Review> reviews, int totalCount) =
                await _reviewRepository.GetMyReviewsByUserIdAsync(searchParams, userId);

            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new MyReviewsDTO
            {
                Reviews = reviews.Adapt<List<MyReviewDTO>>(),
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<MyReviewDetailsDTO> GetMyReviewDetailsByIdAsync(int reviewId, int userId)
        {
            // Validar al usuario
            bool userExists = await _userRepository.ExistsByIdAsync(userId);
            if (!userExists)
            {
                Log.Error(
                    "No se encontró el usuario con ID {UserId} para obtener los detalles de la review.",
                    userId
                );
                throw new KeyNotFoundException($"No se encontró el usuario con ID {userId}.");
            }
            // Obtener los detalles de la review
            Review? reviewDetails = await _reviewRepository.GetMyReviewDetailsByIdAsync(
                reviewId,
                userId
            );
            if (reviewDetails == null)
            {
                Log.Error(
                    "No se encontró la review con ID {ReviewId} para obtener sus detalles.",
                    reviewId
                );
                throw new KeyNotFoundException($"No se encontró la review con ID {reviewId}.");
            }
            // Validar rol y estado del usuario dentro de la review para determinar qué información esconder de forma manual.
            // Si el usuario no ha evaluado a su contraparte, se oculta la información de su contraparte para evitar que pueda acceder a ella antes de completar su evaluación.
            // Esto se hace de forma manual para evitar tener que hacer consultas adicionales a la base de datos para determinar qué información ocultar.
            if (reviewDetails.ApplicantId == userId && !reviewDetails.HasApplicantEvaluatedOfferor)
            {
                Log.Warning(
                    "El usuario con ID {UserId} intentó acceder a los detalles de una review que aún no ha evaluado al oferente.",
                    userId
                );
                reviewDetails.OfferorCommentForApplicant = null;
                reviewDetails.OfferorRatingOfApplicant = null;
            }
            else if (
                reviewDetails.OfferorId == userId
                && !reviewDetails.HasOfferorEvaluatedApplicant
            )
            {
                Log.Warning(
                    "El usuario con ID {UserId} intentó acceder a los detalles de una review que aún no ha evaluado al postulante.",
                    userId
                );
                reviewDetails.ApplicantCommentForOfferor = null;
                reviewDetails.ApplicantRatingOfOfferor = null;
            }
            return reviewDetails.Adapt<MyReviewDetailsDTO>();
        }

        public async Task<GetReviewsDTO> GetAllReviewsForAdminAsync(
            int adminId,
            GetReviewsSearchParamsDTO searchParams
        )
        {
            // Validar que el usuario sea un administrador
            bool userExists = await _userRepository.ExistsByIdAsync(adminId);
            if (!userExists)
            {
                Log.Error(
                    "No se encontró el usuario con ID {AdminId} para ocultar información de la review.",
                    adminId
                );
                throw new KeyNotFoundException($"No se encontró el usuario con ID {adminId}.");
            }
            var isAdmin = await _userRepository.CheckRoleAsync(adminId, RoleNames.Admin);
            if (!isAdmin)
            {
                Log.Error(
                    "El usuario con ID {AdminId} no tiene permisos de administrador para acceder a las reviews.",
                    adminId
                );
                throw new UnauthorizedAccessException(
                    $"El usuario con ID {adminId} no tiene permisos de administrador para acceder a las reviews."
                );
            }

            (List<Review> reviews, int totalCount) =
                await _reviewRepository.GetAllReviewsForAdminAsync(searchParams, adminId);

            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new GetReviewsDTO
            {
                Reviews = reviews.Adapt<List<GetReviewDTO>>(),
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<GetReviewDetailsDTO> GetReviewDetailsForAdminByIdAsync(
            int reviewId,
            int adminId
        )
        {
            // Validar que el usuario sea un administrador
            bool userExists = await _userRepository.ExistsByIdAsync(adminId);
            if (!userExists)
            {
                Log.Error(
                    "No se encontró el usuario con ID {AdminId} para ocultar información de la review.",
                    adminId
                );
                throw new KeyNotFoundException($"No se encontró el usuario con ID {adminId}.");
            }
            var isAdmin = await _userRepository.CheckRoleAsync(adminId, RoleNames.Admin);
            if (!isAdmin)
            {
                Log.Error(
                    "El usuario con ID {AdminId} no tiene permisos de administrador para acceder a las reviews.",
                    adminId
                );
                throw new UnauthorizedAccessException(
                    $"El usuario con ID {adminId} no tiene permisos de administrador para acceder a las reviews."
                );
            }

            Review? reviewDetails = await _reviewRepository.GetReviewDetailsForAdminByIdAsync(
                reviewId
            );
            if (reviewDetails == null)
            {
                Log.Error(
                    "No se encontró la review con ID {ReviewId} para obtener sus detalles.",
                    reviewId
                );
                throw new KeyNotFoundException($"No se encontró la review con ID {reviewId}.");
            }
            return reviewDetails.Adapt<GetReviewDetailsDTO>();
        }
        #endregion

        #region Auxiliary Methods for Review Management
        public async Task<string> HideReviewInfoAsync(
            int adminId,
            int reviewId,
            HideReviewInfoDTO infoDTO
        )
        {
            // Validar al usuario
            bool adminExists = await _userRepository.ExistsByIdAsync(adminId);
            if (!adminExists)
            {
                Log.Error(
                    "No se encontró el usuario con ID {AdminId} para ocultar información de la review.",
                    adminId
                );
                throw new KeyNotFoundException($"No se encontró el usuario con ID {adminId}.");
            }
            bool isAdmin = await _userRepository.CheckRoleAsync(adminId, RoleNames.Admin);
            if (!isAdmin)
            {
                Log.Error(
                    "El usuario con ID {AdminId} no tiene el rol de admin para ocultar información de la review.",
                    adminId
                );
                throw new UnauthorizedAccessException(
                    $"El usuario con ID {adminId} no tiene el rol de admin para ocultar información de la review."
                );
            }
            // Validar la review
            Review? review = await _reviewRepository.GetByIdAsync(
                reviewId,
                new ReviewQueryOptions
                {
                    TrackChanges = true,
                    IncludeApplicant = true,
                    IncludeOfferor = true,
                    IncludeApplication = true,
                }
            );
            if (review == null)
            {
                Log.Error(
                    "No se encontró la review con ID {ReviewId} para ocultar información de la review.",
                    reviewId
                );
                throw new KeyNotFoundException($"No se encontró la review con ID {reviewId}");
            }
            // Ocultar la información de la review según lo especificado en el DTO
            if (
                infoDTO.HideOfferorReviewForApplicant.HasValue
                && (
                    !review.HasOfferorEvaluatedApplicant || review.IsOfferorReviewForApplicantHidden
                )
            )
            {
                Log.Error(
                    "El admin con ID {AdminId} intentó ocultar la información de una review que aún no ha sido completada por el oferente.",
                    adminId
                );
                throw new InvalidOperationException(
                    "No se puede ocultar la información de una review que aún no ha sido completada por el oferente."
                );
            }
            else if (
                infoDTO.HideApplicantReviewForOfferor.HasValue
                && (
                    !review.HasApplicantEvaluatedOfferor || review.IsApplicantReviewForOfferorHidden
                )
            )
            {
                Log.Error(
                    "El admin con ID {AdminId} intentó ocultar la información de una review que aún no ha sido completada por el postulante.",
                    adminId
                );
                throw new InvalidOperationException(
                    "No se puede ocultar la información de una review que aún no ha sido completada por el postulante."
                );
            }
            if (infoDTO.HideOfferorReviewForApplicant == true)
            {
                review.IsOfferorReviewForApplicantHidden = true;
                review.OfferorReviewHiddenReason =
                    infoDTO.OfferorReviewHiddenReason ?? "Información oculta por un administrador.";
            }
            if (infoDTO.HideApplicantReviewForOfferor == true)
            {
                review.IsApplicantReviewForOfferorHidden = true;
                review.ApplicantReviewHiddenReason =
                    infoDTO.ApplicantReviewHiddenReason
                    ?? "Información oculta por un administrador.";
            }
            bool updated = await _reviewRepository.UpdateReviewAsync(review);
            if (!updated)
            {
                Log.Error(
                    "No se pudo actualizar la review con ID {ReviewId} para ocultar información de la review.",
                    review.Id
                );
                throw new InvalidOperationException(
                    "No se pudo actualizar la review para ocultar información de la review"
                );
            }

            return "Información de la review ocultada exitosamente.";
        }

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
            int pendingCount = await _reviewRepository.GetPendingCountOfReviewsByUserIdAsync(
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
    }
}
