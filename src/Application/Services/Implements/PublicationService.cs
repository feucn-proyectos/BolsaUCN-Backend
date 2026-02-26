using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.SpecificUserPublicationsDTO;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Application.Jobs.Interfaces;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Hangfire;
using Mapster;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio para la gestión de publicaciones (Ofertas y Compra/Venta)
    /// </summary>
    public class PublicationService : IPublicationService
    {
        private readonly int _maxAppeals;
        private readonly int _defaultPageSize;
        private readonly IConfiguration _configuration;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReviewService _reviewService;
        private readonly IReviewRepository _reviewRepository;
        private readonly int _daysUntilReviewAutoClose;

        public PublicationService(
            IPublicationRepository publicationRepository,
            IReviewService reviewService,
            IReviewRepository reviewRepository,
            IUserRepository userRepository,
            IConfiguration configuration
        )
        {
            _publicationRepository = publicationRepository;
            _reviewService = reviewService;
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
            _maxAppeals = _configuration.GetValue<int>("PublicationSettings:MaxAppeals");
            _daysUntilReviewAutoClose = _configuration.GetValue<int>(
                "JobsConfiguration:DaysUntilReviewAutoClose"
            );
        }

        #region Metodos para las Ofertas

        public async Task<string> CreateOfferAsync(CreateOfferDTO offerDTO, int userId)
        {
            // Obtener y validar el usuario actual
            User? currentUser = await _userRepository.GetByIdAsync(userId);
            if (currentUser == null)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool isOfferor = await _userRepository.CheckRoleAsync(userId, RoleNames.Offeror);
            if (!isOfferor)
            {
                Log.Error("El usuario con ID {UserId} no tiene permisos de oferente.", userId);
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos para crear ofertas."
                );
            }

            // Validaciones de fechas
            if (offerDTO.EndDate <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "La fecha de finalización (EndDate) debe ser en el futuro."
                );
            }
            if (offerDTO.ApplicationDeadline >= offerDTO.EndDate)
            {
                throw new InvalidOperationException(
                    "La fecha límite de postulación (ApplicationDeadline) debe ser anterior a la fecha de finalización de la oferta."
                );
            }
            // Validar que el usuario no tenga más de 3 reseñas pendientes
            var pendingReviewsCount = 0; //TODO await _reviewService.GetPendingReviewsCountAsync(currentUser);
            Log.Information(
                "Reseñas pendientes para el usuario {userId}: {pendingReviewsCount}",
                userId,
                pendingReviewsCount
            );
            if (pendingReviewsCount >= 3)
            {
                Log.Warning(
                    "Usuario {userId} intentó crear una oferta con {pendingReviewsCount} reseñas pendientes",
                    userId,
                    pendingReviewsCount
                );
                throw new InvalidOperationException(
                    "No se puede crear una oferta mientras se tengan 3 o más reseñas pendientes."
                );
            }

            // Mapeo a la entidad y asignaciones adicionales
            Offer newOffer = offerDTO.Adapt<Offer>();
            newOffer.UserId = currentUser.Id;
            newOffer.ReviewDeadline = newOffer.EndDate.AddDays(_daysUntilReviewAutoClose);
            newOffer.PublicationType = PublicationType.Oferta;

            // Asignar estado de aprobación e isOpen según el rol
            bool isAdmin = await _userRepository.CheckRoleAsync(userId, RoleNames.Admin);
            newOffer.ApprovalStatus = isAdmin ? ApprovalStatus.Aceptada : ApprovalStatus.Pendiente;

            (Offer createdOffer, bool isCreated) =
                await _publicationRepository.CreatePublicationAsync(newOffer);
            if (!isCreated)
            {
                Log.Error("Error al crear la oferta para el usuario {UserId}.", userId);
                throw new Exception("No se pudo crear la oferta. Inténtalo de nuevo.");
            }
            Log.Information(
                "Oferta creada exitosamente. ID: {OfferId}, Título: {Title}, Usuario: {UserId}",
                createdOffer.Id,
                createdOffer.Title,
                currentUser.Id
            );

            // Programar trabajos para controlar el ciclo de vida de la oferta
            // Trabajo para cerrar la postulación, otro para marcar como finalizada e inicializar reseñas, y otro para cerrar reseñas después de un tiempo definido.
            string closeOfferForApplicationsId = BackgroundJob.Schedule<IOfferJobs>(
                job => job.SetAsCloseForApplicationsAsync(createdOffer.Id),
                newOffer.ApplicationDeadline
            );
            string finishWorkAndInitializeReviewsId = BackgroundJob.Schedule<IOfferJobs>(
                job => job.SetAsCompleteAndInitializeReviewsAsync(createdOffer.Id),
                newOffer.EndDate
            );
            string closeReviewsId = BackgroundJob.Schedule<IOfferJobs>(
                job => job.SetAsFinalizedAndCloseReviewsAsync(createdOffer.Id),
                newOffer.ReviewDeadline
            );

            createdOffer.CloseApplicationsJobId = closeOfferForApplicationsId;
            createdOffer.FinishWorkAndInitializeReviewsJobId = finishWorkAndInitializeReviewsId;
            createdOffer.FinalizeAndCloseReviewsJobId = closeReviewsId;
            await _publicationRepository.UpdateAsync(createdOffer);

            return $"Oferta creada exitosamente. Oferta ID: {createdOffer.Id}";
        }

        #endregion

        #region Metodos para Compra/Venta

        /// <summary>
        /// Crea una nueva publicación de compra/venta
        /// </summary>
        public async Task<string> CreateBuySellAsync(CreateBuySellDTO buySellDTO, int currentUserId)
        {
            // Obtener y validar el usuario actual
            var currentUser =
                await _userRepository.GetByIdAsync(currentUserId)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");
            var isAdmin =
                currentUser.UserType == UserType.Administrador
                    ? true
                    : throw new UnauthorizedAccessException(
                        "Solo los administradores pueden crear publicaciones de compra/venta directamente publicadas."
                    );
            // Validar que el usuario no tenga más de 3 reseñas pendientes como oferente
            var pendingReviewsCount = await _reviewService.GetPendingReviewsCountAsync(
                currentUser,
                RoleNames.Offeror
            );
            if (pendingReviewsCount >= 3)
            {
                Log.Warning(
                    "Usuario {userId} intentó crear publicación de compra/venta con {PendingCount} reseñas pendientes",
                    currentUser.Id,
                    pendingReviewsCount
                );
                throw new InvalidOperationException(
                    "No se puede crear una publicacion de compra/venta mientras se tengan 3 o más reseñas pendientes."
                );
            }
            // Mapeo principal DTO a Entidad
            BuySell newBuySell = buySellDTO.Adapt<BuySell>();
            // Mapeo adicional y asignaciones
            newBuySell.UserId = currentUser.Id;
            newBuySell.PublicationType = PublicationType.CompraVenta;

            newBuySell.ApprovalStatus = isAdmin
                ? ApprovalStatus.Aceptada
                : ApprovalStatus.Pendiente;

            var createdBuySellResult = true; //await _buySellRepository.CreateBuySellAsync(newBuySell);

            Log.Information(
                "Publicación de compra/venta creada exitosamente. ID: {BuySellId}, Título: {Title}, Usuario: {UserId}",
                createdBuySellResult,
                newBuySell.Title,
                currentUser.Id
            );

            return $"Publicación de compra/venta creada exitosamente. Publicación ID: {createdBuySellResult}";
        }

        #endregion

        #region New Methods

        public async Task<MyPublicationsDTO> GetMyPublicationsAsync(
            int offerorId,
            MyPublicationsSeachParamsDTO searchParams
        )
        {
            // Validar usuario
            bool userExists = await _userRepository.ExistsByIdAsync(offerorId);
            if (!userExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", offerorId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            var (myPublications, totalCount) =
                await _publicationRepository.GetMyPublicationsFilteredByUserIdAsync(
                    offerorId,
                    searchParams
                );

            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            MyPublicationsDTO myPublicationsDTO = new MyPublicationsDTO
            {
                Publications = myPublications.Adapt<List<PublicationForOfferorDTO>>(),
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
            return myPublicationsDTO;
        }

        public async Task<UserPublicationsForAdminDTO> GetPublicationByUserIdAsync(
            int adminId,
            int userId,
            UserPublicationsSearchParamsDTO searchParams
        )
        {
            // Validar usuario admin
            bool adminExists = await _userRepository.ExistsByIdAsync(adminId);
            if (!adminExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", adminId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool isAdmin = await _userRepository.CheckRoleAsync(adminId, RoleNames.Admin);
            if (!isAdmin)
            {
                Log.Warning(
                    "El usuario con ID {UserId} no tiene permisos de administrador.",
                    adminId
                );
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos para acceder a esta información."
                );
            }
            // Validar usuario objetivo
            bool userExists = await _userRepository.ExistsByIdAsync(userId);
            if (!userExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            var (publications, totalCount) =
                await _publicationRepository.GetPublicationsByUserIdFilteredAsync(
                    userId,
                    searchParams
                );

            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            UserPublicationsForAdminDTO userPublicationsForAdminDTO = new()
            {
                Publications = publications.Adapt<List<UserPublicationForAdminDTO>>(),
                TotalPages = totalPages,
                PageNumber = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
            return userPublicationsForAdminDTO;
        }

        public async Task<OffersForApplicantDTO> GetOffersAsync(
            ExploreOffersSearchParamsDTO searchParams,
            int? userId = null
        )
        {
            var (offers, totalCount) = await _publicationRepository.GetOffersFilteredAsync(
                searchParams,
                userId
            );

            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            OffersForApplicantDTO offersForApplicantDTO = new OffersForApplicantDTO
            {
                Offers = offers.Adapt<List<OfferForApplicantDTO>>(),
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
            return offersForApplicantDTO;
        }

        public async Task<OfferDetailsForApplicantDTO> GetOfferDetailsForApplicantAsync(
            int offerId,
            int applicantId
        )
        {
            // Validacion de usuario
            bool userExists = await _userRepository.ExistsByIdAsync(applicantId);
            if (!userExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", applicantId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool isApplicant = await _userRepository.CheckRoleAsync(
                applicantId,
                RoleNames.Applicant
            );
            if (!isApplicant)
            {
                Log.Warning(
                    "El usuario con ID {UserId} no tiene permisos de postulante.",
                    applicantId
                );
            }
            // Validacion de oferta y estado
            var offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                offerId,
                new PublicationQueryOptions { IncludeUser = true, IncludeApplications = true }
            );
            if (offer == null)
            {
                Log.Error("Oferta con ID {OfferId} no encontrada.", offerId);
                throw new KeyNotFoundException("Oferta no encontrada.");
            }
            if (offer.ApprovalStatus != ApprovalStatus.Aceptada)
            {
                Log.Warning(
                    "La oferta con ID {OfferId} no está disponible para postulación. Estado actual: {Status}",
                    offerId,
                    offer.ApprovalStatus
                );
                throw new InvalidOperationException(
                    "La oferta no está disponible para postulación."
                );
            }
            // Validacion de postulación previa
            bool hasApplied = offer.Applications.Any(app => app.StudentId == applicantId);
            if (hasApplied)
            {
                Log.Warning(
                    "El usuario con ID {UserId} ya ha postulado a la oferta con ID {OfferId}.",
                    applicantId,
                    offerId
                );
                //throw new InvalidOperationException("Ya has postulado a esta oferta.");
            }

            OfferDetailsForApplicantDTO offerDetails = offer.Adapt<OfferDetailsForApplicantDTO>();
            // Asignar campos adicionales de contacto
            if (isApplicant)
            {
                offerDetails.ContactEmail = offer.User.Email!;
                offerDetails.ContactPhoneNumber = offer.User.PhoneNumber!;
                offerDetails.AdditionalContactEmail = offer.AdditionalContactEmail;
                offerDetails.AdditionalContactPhoneNumber = offer.AdditionalContactPhoneNumber;
                offerDetails.HasApplied = hasApplied;
            }
            else
            {
                offerDetails.ContactEmail = string.Empty;
                offerDetails.ContactPhoneNumber = string.Empty;
                offerDetails.AdditionalContactEmail = null;
                offerDetails.AdditionalContactPhoneNumber = null;
            }
            return offerDetails;
        }

        public async Task<OfferDetailsForPublicDTO> GetOfferDetailsForPublicAsync(int offerId)
        {
            var offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                offerId,
                new PublicationQueryOptions { IncludeUser = true }
            );
            if (offer == null || offer.ApprovalStatus != ApprovalStatus.Aceptada)
            {
                Log.Error("Oferta con ID {OfferId} no encontrada o no está aceptada.", offerId);
                throw new KeyNotFoundException("Oferta no encontrada.");
            }
            OfferDetailsForPublicDTO offerDetails = offer.Adapt<OfferDetailsForPublicDTO>();
            return offerDetails;
        }

        public async Task<MyPublicationDetailsDTO> GetPublicationDetailsForOffererAsync(
            int publicationId,
            int offerorId
        )
        {
            // Validar usuario
            bool userExists = await _userRepository.ExistsByIdAsync(offerorId);
            if (!userExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", offerorId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            // Revisar el tipo de publicacion para despues cargar las relaciones correspondientes.
            bool? isOffer = await _publicationRepository.CheckType(
                publicationId,
                PublicationType.Oferta
            );
            if (isOffer == null)
            {
                Log.Error("Publicación con ID {PublicationId} no encontrada.", publicationId);
                throw new KeyNotFoundException("Publicación no encontrada.");
            }

            Publication? publication = (bool)isOffer
                ? await _publicationRepository.GetPublicationByIdAsync<Offer>(
                    publicationId,
                    new PublicationQueryOptions { IncludeUser = true, IncludeApplications = true }
                )
                : await _publicationRepository.GetPublicationByIdAsync<BuySell>(
                    publicationId,
                    new PublicationQueryOptions { IncludeUser = true, IncludeImages = true }
                );
            if (publication == null || publication.UserId != offerorId)
            {
                Log.Error(
                    "Publicación con ID {PublicationId} no encontrada para el usuario {UserId}.",
                    publicationId,
                    offerorId
                );
                throw new KeyNotFoundException("Publicación no encontrada.");
            }
            MyPublicationDetailsDTO publicationDetails =
                publication.Adapt<MyPublicationDetailsDTO>();

            return publicationDetails;
        }

        public async Task UpdatePublicationStatusAsync(
            Publication publication,
            ApprovalStatus newStatus,
            string? rejectionReason = null
        )
        {
            var oldStatus = publication.ApprovalStatus;

            // Validar flujo de estados
            if (oldStatus == newStatus)
            {
                Log.Warning(
                    "El estado de la publicación con ID {PublicationId} ya es {Status}. No se realiza ningún cambio.",
                    publication.Id,
                    newStatus
                );
                return;
            }
            else if (oldStatus != ApprovalStatus.Pendiente)
            {
                Log.Error(
                    "No se puede cambiar el estado de la publicación con ID {PublicationId} desde {OldStatus} a {NewStatus}.",
                    publication.Id,
                    oldStatus,
                    newStatus
                );
                throw new InvalidOperationException(
                    "Solo se pueden aprobar o rechazar publicaciones que están en estado 'Pendiente'."
                );
            }
            publication.ApprovalStatus = newStatus;
            if (newStatus == ApprovalStatus.Rechazada)
                publication.RejectedByAdminReason =
                    rejectionReason ?? "No se proporcionó una razón de rechazo.";
            await _publicationRepository.UpdateAsync(publication);
            if (publication.ApprovalStatus == oldStatus)
            {
                Log.Warning(
                    "El estado de la publicación con ID {PublicationId} no cambió. Estado actual: {Status}",
                    publication.Id,
                    publication.ApprovalStatus
                );
            }
            else
            {
                Log.Information(
                    "Estado de la publicación con ID {PublicationId} actualizado a {Status}",
                    publication.Id,
                    publication.ApprovalStatus
                );
            }
        }

        public async Task<PublicationsForAdminDTO> GetAllPublicationsForAdminAsync(
            int adminId,
            PublicationsForAdminSearchParamsDTO searchParamsDTO
        )
        {
            // Validar usuario
            bool userExists = await _userRepository.ExistsByIdAsync(adminId);
            if (!userExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", adminId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool isAdmin = await _userRepository.CheckRoleAsync(adminId, RoleNames.Admin);
            if (!isAdmin)
            {
                Log.Warning(
                    "El usuario con ID {UserId} no tiene permisos de administrador.",
                    adminId
                );
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos para acceder a esta información."
                );
            }

            // Obtener publicaciones con filtrado, búsqueda, ordenamiento y paginación
            var (publications, totalCount) =
                await _publicationRepository.GetAllPublicationsFilteredForAdminAsync(
                    searchParamsDTO
                );

            int currentPage = searchParamsDTO.PageNumber;
            int pageSize = searchParamsDTO.PageSize ?? _defaultPageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            PublicationsForAdminDTO publicationsForAdminDTO = new PublicationsForAdminDTO
            {
                Publications = publications.Adapt<List<PublicationForAdminDTO>>(),
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
            return publicationsForAdminDTO;
        }

        public async Task<PublicationDetailsForAdminDTO> GetPublicationDetailsForAdminByIdAsync(
            int publicationId,
            int adminId
        )
        {
            // Validar usuario
            bool userExists = await _userRepository.ExistsByIdAsync(adminId);
            if (!userExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", adminId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool isAdmin = await _userRepository.CheckRoleAsync(adminId, RoleNames.Admin);
            if (!isAdmin)
            {
                Log.Warning(
                    "El usuario con ID {UserId} no tiene permisos de administrador.",
                    adminId
                );
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos para acceder a esta información."
                );
            }

            bool? isOffer = await _publicationRepository.CheckType(
                publicationId,
                PublicationType.Oferta
            );
            if (isOffer == null)
            {
                Log.Error("Publicación con ID {PublicationId} no encontrada.", publicationId);
                throw new KeyNotFoundException("Publicación no encontrada.");
            }

            Publication? publication = (bool)isOffer
                ? await _publicationRepository.GetPublicationByIdAsync<Offer>(
                    publicationId,
                    new PublicationQueryOptions { IncludeUser = true, IncludeApplications = true }
                )
                : await _publicationRepository.GetPublicationByIdAsync<BuySell>(
                    publicationId,
                    new PublicationQueryOptions { IncludeUser = true, IncludeImages = true }
                );
            if (publication == null)
            {
                Log.Error("Publicación con ID {PublicationId} no encontrada.", publicationId);
                throw new KeyNotFoundException("Publicación no encontrada.");
            }
            PublicationDetailsForAdminDTO publicationDetails =
                publication.Adapt<PublicationDetailsForAdminDTO>();
            return publicationDetails;
        }

        public async Task<string> AppealRejectedPublicationAsync(
            int publicationId,
            int offerorId,
            AppealRejectionDTO appealDTO
        )
        {
            // Validacion de usuario
            bool offerorExists = await _userRepository.ExistsByIdAsync(offerorId);
            if (!offerorExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", offerorId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            // Validacion de publicacion y propiedad
            bool isOffer =
                await _publicationRepository.CheckType(publicationId, PublicationType.Oferta)
                ?? false;
            string result;
            if (isOffer)
            {
                bool appealResult = await EditPublicationForAppealAsync<Offer>(
                    publicationId,
                    appealDTO
                );
                if (!appealResult)
                {
                    Log.Error("Error al apelar la oferta con ID {PublicationId}.", publicationId);
                    throw new Exception("No se pudo apelar la oferta. Inténtalo de nuevo.");
                }
                result = appealResult
                    ? "La apelación de la oferta ha sido enviada exitosamente."
                    : "No se pudo enviar la apelación de la oferta. Inténtalo de nuevo.";
            }
            else
            {
                bool appealResult = await EditPublicationForAppealAsync<BuySell>(
                    publicationId,
                    appealDTO
                );
                if (!appealResult)
                {
                    Log.Error(
                        "Error al apelar la publicación de compra/venta con ID {PublicationId}.",
                        publicationId
                    );
                    throw new Exception(
                        "No se pudo apelar la publicación de compra/venta. Inténtalo de nuevo."
                    );
                }
                result = appealResult
                    ? "La apelación de la publicación de compra/venta ha sido enviada exitosamente."
                    : "No se pudo enviar la apelación de la publicación de compra/venta. Inténtalo de nuevo.";
            }
            //TODO: Enviar notification al admin para revisar la apelación
            return result;
        }

        public async Task<string> AdvanceOfferManuallyAsync(int publicationId, int offerorId)
        {
            // Validacion de usuario
            bool userExists = await _userRepository.ExistsByIdAsync(offerorId);
            if (!userExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", offerorId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool isOfferor = await _userRepository.CheckRoleAsync(offerorId, RoleNames.Offeror);
            if (!isOfferor)
            {
                Log.Error("El usuario con ID {UserId} no tiene permisos de oferente.", offerorId);
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos para cerrar esta publicación."
                );
            }
            // Validacion de publicacion y propiedad
            var publication = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                publicationId,
                new PublicationQueryOptions { IncludeApplications = true }
            );
            if (publication == null)
            {
                Log.Error(
                    "Publicación del tipo oferta con ID {PublicationId} no encontrada.",
                    publicationId
                );
                throw new KeyNotFoundException("Publicación no encontrada.");
            }

            // Validacion de estado actual
            if (!publication.IsWorkInProgress && !publication.IsAcceptingApplications)
            {
                Log.Error(
                    "La publicación con ID {PublicationId} no está en un estado válido para avanzarse manualmente.",
                    publicationId
                );
                throw new InvalidOperationException(
                    "Solo se pueden avanzar manualmente las publicaciones que están en estado 'Realizando Trabajo' o 'Recibiendo Postulaciones'."
                );
            }
            // Flujo de cierre (Avanza un paso hacia adelante)
            if (publication.IsAcceptingApplications)
            {
                if (!publication.Applications.Any(a => a.Status == ApplicationStatus.Aceptada))
                {
                    Log.Error(
                        "No se puede avanzar la publicación con ID {PublicationId} porque no tiene postulantes aceptados.",
                        publicationId
                    );
                    throw new InvalidOperationException(
                        "No se puede avanzar la publicación porque no tiene postulantes aceptados. Si deseas avanzar la publicación sin postulantes, por favor considere cancelar la publicación manualmente en vez de avanzar el estado de la publicación."
                    );
                }
                // Cancelar el trabajo y llamar manualmente al metodo que cierra la postulación para .
                BackgroundJob.Delete(publication.CloseApplicationsJobId);
                await CloseOfferForApplicationsAsync(publication.Id);
            }
            else if (publication.IsWorkInProgress)
            {
                publication.ReviewDeadline = DateTime.UtcNow.AddDays(_daysUntilReviewAutoClose);
                // Cancelar el trabajo y llamar manualmente al metodo que finaliza el trabajo e inicializa las reseñas para esta publicación.
                BackgroundJob.Delete(publication.FinishWorkAndInitializeReviewsJobId);
                await CompleteAndInitializeReviewsAsync(publication.Id);
                // Reprogramar el trabajo que cierra las reseñas para que se ejecute después de los días configurados a partir de ahora, ya que al ejecutar manualmente el método anterior, la publicación se marca como finalizada y las reseñas se inicializan, por lo que el trabajo programado para cerrar las reseñas debe ejecutarse después de esto.
                BackgroundJob.Reschedule(
                    publication.FinalizeAndCloseReviewsJobId,
                    DateTimeOffset.UtcNow.AddDays(_daysUntilReviewAutoClose)
                );
            }

            //TODO: Enviar notificaciones a los postulantes informando del cambio de estado de la oferta.

            return $"Publicación avanzada exitosamente. Nuevo estado: {(publication.IsWorkInProgress ? "Realizando Trabajo" : "Calificaciones en Proceso")}.";
        }

        public async Task<string> CancelOfferManuallyAsync(
            int publicationId,
            int offerorId,
            ClosePublicationRequestDTO? requestDTO = null
        )
        {
            // Validacion de usuario
            bool userExists = await _userRepository.ExistsByIdAsync(offerorId);
            if (!userExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", offerorId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool isOfferor = await _userRepository.CheckRoleAsync(offerorId, RoleNames.Offeror);
            if (!isOfferor)
            {
                Log.Error("El usuario con ID {UserId} no tiene permisos de oferente.", offerorId);
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos para cancelar esta publicación."
                );
            }
            // Validacion de publicacion y propiedad
            var publication = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                publicationId,
                new PublicationQueryOptions { TrackChanges = true, IncludeUser = true }
            );
            if (publication == null)
            {
                Log.Error(
                    "Publicación del tipo oferta con ID {PublicationId} no encontrada.",
                    publicationId
                );
                throw new KeyNotFoundException("Publicación no encontrada.");
            }
            // Validacion de permisos en caso de que la publicacion no le pertenezca al usuario
            bool isAdmin = await _userRepository.CheckRoleAsync(offerorId, RoleNames.Admin);
            if (publication.UserId != offerorId && !isAdmin)
            {
                Log.Error(
                    "Usuario con ID {UserId} no es el propietario de la publicación con ID {PublicationId}.",
                    offerorId,
                    publicationId
                );
                throw new UnauthorizedAccessException(
                    "No tienes permiso para cerrar esta publicación."
                );
            }
            // Validacion de estado actual
            if (!publication.IsAcceptingApplications)
            {
                Log.Error(
                    "La publicación con ID {PublicationId} no está en un estado válido para cancelarse manualmente.",
                    publicationId
                );
                throw new InvalidOperationException(
                    "Solo se pueden cancelar manualmente las publicaciones que están en estado 'Recibiendo Postulaciones'."
                );
            }
            // Cierre de la oferta
            publication.CancelOffer();
            if (isAdmin && !string.IsNullOrEmpty(requestDTO!.ClosedByAdminReason))
                publication.ClosedByAdminReason = requestDTO.ClosedByAdminReason;
            bool updateResult = await _publicationRepository.UpdateAsync(publication);
            if (!updateResult)
            {
                Log.Error(
                    "Error al cancelar la publicación con ID {PublicationId}.",
                    publicationId
                );
                throw new Exception("No se pudo cancelar la publicación. Inténtalo de nuevo.");
            }

            //TODO: Enviar notificaciones a los postulantes informando de la cancelacion de la oferta.
            //TODO: Enviar notificacion al oferente si la accion fue realizada por un administrador.
            // Cancelar el trabajo programado para crear las calificaciones iniciales ya que el trabajo ya no se realizara
            BackgroundJob.Delete(publication.FinishWorkAndInitializeReviewsJobId!);
            BackgroundJob.Delete(publication.FinalizeAndCloseReviewsJobId!);

            return "Publicación cancelada exitosamente.";
        }
        #endregion

        private async Task<bool> EditPublicationForAppealAsync<T>(
            int publicationId,
            AppealRejectionDTO appealDTO
        )
            where T : Publication
        {
            // Validar que la publicación exista y sea del tipo correcto
            var publication = await _publicationRepository.GetPublicationByIdAsync<T>(
                publicationId,
                new PublicationQueryOptions { TrackChanges = true }
            );
            if (publication == null)
            {
                Log.Error("Publicación con ID {PublicationId} no encontrada.", publicationId);
                throw new KeyNotFoundException("Publicación no encontrada.");
            }
            // Validar flujo de estados
            if (publication.ApprovalStatus != ApprovalStatus.Rechazada)
            {
                Log.Error(
                    "No se puede apelar la publicación con ID {PublicationId} porque su estado actual es {Status}.",
                    publicationId,
                    publication.ApprovalStatus
                );
                throw new InvalidOperationException(
                    "Solo se pueden apelar publicaciones que están en estado 'Rechazada'."
                );
            }
            // Guardar las fechas originales en caso de que la publicación sea una oferta, para luego comparar si fueron modificadas en la apelación y reprogramar los trabajos correspondientes en caso de ser necesario.
            DateTime? originalEndDate;
            DateTime? originalApplicationDeadline;
            if (publication is Offer offer)
            {
                originalEndDate = offer.EndDate;
                originalApplicationDeadline = offer.ApplicationDeadline;
            }
            else
            {
                originalEndDate = null;
                originalApplicationDeadline = null;
            }
            publication = appealDTO.Adapt(publication);
            publication.ApprovalStatus = ApprovalStatus.Pendiente;
            publication.AppealCount++;

            if (publication is Offer updatedOffer)
            {
                // Si la oferta tenía una fecha de cierre de postulación original y esta fue modificada en la apelación, reprograma el trabajo de cierre de postulación.
                if (
                    originalApplicationDeadline != null
                    && originalApplicationDeadline != updatedOffer.ApplicationDeadline
                )
                {
                    BackgroundJob.Reschedule(
                        updatedOffer.CloseApplicationsJobId,
                        updatedOffer.ApplicationDeadline
                    );
                }
                // Si la oferta tenía una fecha de finalización original y esta fue modificada en la apelación, reprograma el trabajo de creación de reseñas.
                if (originalEndDate != null && originalEndDate != updatedOffer.EndDate)
                {
                    updatedOffer.ReviewDeadline = updatedOffer.EndDate.AddDays(
                        _daysUntilReviewAutoClose
                    );
                    BackgroundJob.Reschedule(
                        updatedOffer.FinishWorkAndInitializeReviewsJobId,
                        updatedOffer.EndDate
                    );
                    BackgroundJob.Reschedule(
                        updatedOffer.FinalizeAndCloseReviewsJobId,
                        updatedOffer.ReviewDeadline
                    );
                }
            }
            bool updateResult = await _publicationRepository.UpdateAsync(publication);
            if (!updateResult)
            {
                Log.Error(
                    "Error al actualizar la publicación con ID {PublicationId}.",
                    publicationId
                );
                throw new Exception("No se pudo actualizar la publicación. Inténtalo de nuevo.");
            }
            return true;
        }

        #region Background Jobs

        public async Task CloseOfferForApplicationsAsync(int offerId)
        {
            // Validar oferta
            Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                offerId,
                new PublicationQueryOptions { TrackChanges = true, IncludeApplications = true }
            );
            if (offer == null)
            {
                Log.Error("Oferta con ID {OfferId} no encontrada para cierre automático.", offerId);
                throw new KeyNotFoundException("Oferta no encontrada.");
            }
            // Validar estado actual: Si aun esta en estado pendiente significa no lo ha revisado un administrador.
            // En este caso la oferta se cancela y se le indica al oferente que someta una nueva oferta o apele.
            if (offer.IsInAdminReview)
            {
                Log.Warning(
                    "La oferta con ID {OfferId} está en revisión administrativa por lo que se mueve a rechazada automaticamente.",
                    offerId
                );
                offer.ApprovalStatus = ApprovalStatus.Rechazada;
                offer.RejectedByAdminReason =
                    "La oferta fue rechazada automáticamente por no ser revisada por un administrador dentro del plazo establecido.";
                bool resultRejected = await _publicationRepository.UpdateAsync(offer);
                if (!resultRejected)
                {
                    Log.Error(
                        "Error al actualizar la oferta con ID {OfferId} a estado rechazada durante el cierre automático.",
                        offerId
                    );
                    throw new Exception("No se pudo actualizar la oferta. Inténtalo de nuevo.");
                }
                // Cancelar los trabajos programados ya que la oferta no se realizara
                BackgroundJob.Delete(offer.FinishWorkAndInitializeReviewsJobId!);
                BackgroundJob.Delete(offer.FinalizeAndCloseReviewsJobId!);
                return;
            }
            // Validar estado actual: Si no esta en estado de recibiendo postulaciones, no se puede cerrar para postulaciones ya que algo salio mal. No deberia pasar pero se valida por seguridad.
            if (!offer.IsAcceptingApplications)
            {
                Log.Error(
                    "La oferta con ID {OfferId} no está en estado 'Recibiendo Postulaciones' para cierre automático. Estado actual: {Status}",
                    offerId,
                    offer.CurrentStatus.ToString()
                );
                throw new InvalidOperationException(
                    "La oferta no está en estado 'Recibiendo Postulaciones' para cierre automático."
                );
            }
            // Validar postulantes aceptadas
            // Si no hay postulantes aceptados, se cancela la oferta y se le indica al oferente que someta una nueva oferta o apele. Si hay postulantes aceptados, se avanza al siguiente estado de realizando trabajo.
            var acceptedApplications = offer
                .Applications.Where(a => a.Status == ApplicationStatus.Aceptada)
                .ToList();
            if (acceptedApplications.Count == 0)
            {
                Log.Warning(
                    "La oferta con ID {OfferId} no tiene postulantes aceptados al momento del cierre automático.",
                    offerId
                );
                offer.CancelOffer();
                // Cancelar los trabajos programados ya que la oferta no se realizara
                BackgroundJob.Delete(offer.FinishWorkAndInitializeReviewsJobId!);
                BackgroundJob.Delete(offer.FinalizeAndCloseReviewsJobId!);
            }
            else
            {
                offer.StartWork();
            }
            bool updateResult = await _publicationRepository.UpdateAsync(offer);
            if (!updateResult)
            {
                Log.Error(
                    "Error al cerrar la oferta con ID {OfferId} para postulaciones automáticamente.",
                    offerId
                );
                throw new Exception("No se pudo cerrar la oferta. Inténtalo de nuevo.");
            }
        }

        public async Task CompleteAndInitializeReviewsAsync(int offerId)
        {
            Log.Information(
                "Ejecutando trabajo para finalizar la oferta con ID {OfferId} y crear reseñas automáticamente.",
                offerId
            );
            // Validar oferta
            Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                offerId,
                new PublicationQueryOptions { TrackChanges = true, IncludeApplications = true }
            );
            if (offer == null)
            {
                Log.Error(
                    "Oferta con ID {OfferId} no encontrada para finalizar y crear reseñas automáticamente.",
                    offerId
                );
                throw new KeyNotFoundException("Oferta no encontrada.");
            }
            // Validar estado actual
            if (!offer.IsWorkInProgress)
            {
                Log.Error(
                    "La oferta con ID {OfferId} no está en estado 'Realizando Trabajo' para finalizar y crear reseñas automáticamente. Estado actual: {Status}",
                    offerId,
                    offer.CurrentStatus.ToString()
                );
                throw new InvalidOperationException(
                    "La oferta no está en estado 'Realizando Trabajo' para finalizar y crear reseñas automáticamente."
                );
            }
            Log.Information(
                "Finalizando la oferta con ID {OfferId} y creando reseñas automáticamente.",
                offerId
            );
            // Finalizar el trabajo y crear las reseñas iniciales
            offer.CompleteWork();
            bool updateResult = await _publicationRepository.UpdateAsync(offer);
            if (!updateResult)
            {
                Log.Error(
                    "Error al finalizar la oferta con ID {OfferId} y crear reseñas automáticamente.",
                    offerId
                );
                throw new Exception("No se pudo finalizar la oferta. Inténtalo de nuevo.");
            }
            int reviewsCreated = await _reviewService.CreateInitialReviewsForCompletedOfferAsync(
                offer.Id
            );
            Log.Information(
                "Oferta con ID {OfferId} finalizada y {ReviewCount} reseñas iniciales creadas automáticamente.",
                offerId,
                reviewsCreated
            );
        }

        public async Task FinalizeAndCloseReviewsAsync(int offerId)
        {
            Log.Information(
                "Ejecutando trabajo para finalizar y cerrar reseñas de la oferta con ID {OfferId} automáticamente.",
                offerId
            );

            // Validar oferta
            Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                offerId,
                new PublicationQueryOptions { TrackChanges = true, IncludeApplications = true }
            );
            if (offer == null)
            {
                Log.Error(
                    "Oferta con ID {OfferId} no encontrada para finalizar y cerrar reseñas automáticamente.",
                    offerId
                );
                throw new KeyNotFoundException("Oferta no encontrada.");
            }
            // Validar estado actual
            if (!offer.IsAwaitingReviews)
            {
                Log.Error(
                    "La oferta con ID {OfferId} no está en estado 'Calificaciones en Proceso' para finalizar y cerrar reseñas automáticamente. Estado actual: {Status}",
                    offerId,
                    offer.CurrentStatus.ToString()
                );
                throw new InvalidOperationException(
                    "La oferta no está en estado 'Calificaciones en Proceso' para finalizar y cerrar reseñas automáticamente."
                );
            }
            Log.Information(
                "Finalizando y cerrando reseñas de la oferta con ID {OfferId} automáticamente.",
                offerId
            );
            // Validar que las reseñas existan y estén en estado completado
            var reviews = await _reviewRepository.GetReviewsByOfferIdAsync(offer.Id);
            if (reviews.Count == 0)
            {
                Log.Warning(
                    "La oferta con ID {OfferId} no tiene reseñas asociadas al momento de finalizar y cerrar reseñas automáticamente.",
                    offerId
                );
                throw new InvalidOperationException(
                    "No se pueden finalizar y cerrar las reseñas porque no existen reseñas asociadas a esta oferta."
                );
            }
            // Obtener todos los usuarios involucrados
            var usersInvolved = new HashSet<User> { offer.User };
            foreach (var review in reviews)
            {
                if (review.Applicant != null)
                    usersInvolved.Add(review.Applicant);

                review.ReviewClosedAt = DateTime.UtcNow;
            }
            // Calcular las calificaciones finales promediando las calificaciones del oferente hacia el postulante y viceversa.
            foreach (User user in usersInvolved)
            {
                await _reviewRepository.CalculateUserRating(user);
            }

            // Finalizar el ciclo de vida de la oferta

            offer.FinalizeOffer();
            int updateResult = await _publicationRepository.SaveChangesAsync();
            if (updateResult == 0)
            {
                Log.Error(
                    "Error al finalizar la oferta con ID {OfferId} durante el proceso de finalización y cierre automático de reseñas.",
                    offerId
                );
                throw new Exception("No se pudo finalizar la oferta. Inténtalo de nuevo.");
            }
            Log.Information(
                "Oferta con ID {OfferId} finalizada y reseñas cerradas automáticamente.",
                offerId
            );
        }
        #endregion
    }
}
