using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
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

        public PublicationService(
            IPublicationRepository publicationRepository,
            IReviewService reviewService,
            IUserRepository userRepository,
            IConfiguration configuration
        )
        {
            _publicationRepository = publicationRepository;
            _reviewService = reviewService;
            _userRepository = userRepository;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
            _maxAppeals = _configuration.GetValue<int>("PublicationSettings:MaxAppeals");
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
            var pendingReviewsCount = await _reviewService.GetPendingReviewsCountAsync(currentUser);
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
            newOffer.PublicationType = PublicationType.Oferta;

            // Asignar estado de aprobación e isOpen según el rol
            bool isAdmin = await _userRepository.CheckRoleAsync(userId, RoleNames.Admin);
            newOffer.ApprovalStatus = isAdmin ? ApprovalStatus.Aceptada : ApprovalStatus.Pendiente;

            bool createOfferResult = await _publicationRepository.CreatePublicationAsync(newOffer);

            Log.Information(
                "Oferta creada exitosamente. ID: {OfferId}, Título: {Title}, Usuario: {UserId}",
                createOfferResult,
                newOffer.Title,
                currentUser.Id
            );

            return $"Oferta creada exitosamente. Oferta ID: {createOfferResult}";
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
            // Validar que el usuario no tenga más de 3 reseñas pendientes
            var pendingReviewsCount = await _reviewService.GetPendingReviewsCountAsync(currentUser);
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


        // --- IMPLEMENTACIÓN PENDING ("InProcess") ---
        // --- IMPLEMENTACIÓN PENDING ("InProcess") CORREGIDA ---

        #region Metodos para publicaciones en general

        //! NEEDS MORE RESEARCH - NO FLOW DOCUMENTATION FOUND
        /// <summary>
        /// Allows a user to appeal a rejection if limits allow.
        /// </summary>
        public async Task<GenericResponse<string>> AppealPublicationAsync(
            int publicationId,
            int userId
        )
        {
            var publication = await _publicationRepository.GetPublicationByIdAsync<Publication>(
                publicationId
            );

            // 1. Validate existence -> 404 Not Found
            if (publication == null)
                throw new KeyNotFoundException("La publicación no existe.");

            // 2. Validate ownership -> 401 Unauthorized (Based on JobApplicationService pattern)
            if (publication.UserId != userId)
                throw new UnauthorizedAccessException(
                    "No tienes permiso para apelar esta publicación."
                );

            // 3. Validate status -> 409 Conflict (InvalidOperationException maps to 409 in your middleware)
            if (publication.ApprovalStatus != ApprovalStatus.Rechazada)
                throw new InvalidOperationException(
                    "Solo se pueden apelar publicaciones que han sido rechazadas."
                );

            // 4. Validate limits -> 409 Conflict
            if (publication.AppealCount >= _maxAppeals)
            {
                throw new InvalidOperationException(
                    $"Has alcanzado el límite máximo de {_maxAppeals} apelaciones para esta publicación."
                );
            }

            // 5. Process appeal
            publication.ApprovalStatus = ApprovalStatus.Pendiente;
            //publication.UserAppealJustification = dto.Justification; Clients dont want any justification provided, just notice that the publiction has been resubmitted
            publication.AppealCount++;

            await _publicationRepository.UpdateAsync(publication);

            return new GenericResponse<string>(
                $"Apelación enviada exitosamente. Intento {publication.AppealCount} de {_maxAppeals}. Un administrador revisará tu caso."
            );
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

        public async Task<OffersForApplicantDTO> GetOffersAsync(
            ExploreOffersSearchParamsDTO searchParams
        )
        {
            var (offers, totalCount) = await _publicationRepository.GetOffersFilteredAsync(
                searchParams
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
                throw new InvalidOperationException("Ya has postulado a esta oferta.");
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
                publication.AdminRejectionReason =
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

        public async Task<string> CloseOfferManuallyAsync(int publicationId, int offerorId)
        {
            // Validacion de usuario
            bool userExists = await _userRepository.ExistsByIdAsync(offerorId);
            if (!userExists)
            {
                Log.Error("Usuario con ID {UserId} no encontrado.", offerorId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            // Validacion de publicacion y propiedad
            var publication = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                publicationId,
                new PublicationQueryOptions { IncludeUser = true }
            );
            if (publication == null)
            {
                Log.Error("Publicación con ID {PublicationId} no encontrada.", publicationId);
                throw new KeyNotFoundException("Publicación no encontrada.");
            }
            // Validacion de permisos (propietario o admin)
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
            if (publication.ApprovalStatus != ApprovalStatus.Aceptada)
            {
                Log.Error(
                    "No se puede cerrar la publicación con ID {PublicationId} porque su estado actual es {Status}.",
                    publicationId,
                    publication.ApprovalStatus
                );
                throw new InvalidOperationException(
                    "Solo se pueden cerrar manualmente las publicaciones que están en estado 'Aceptada'."
                );
            }
            // Cierre de la oferta
            publication.ApprovalStatus = ApprovalStatus.Cerrada;
            bool updateResult = await _publicationRepository.UpdateAsync(publication);
            if (!updateResult)
            {
                Log.Error("Error al cerrar la publicación con ID {PublicationId}.", publicationId);
                throw new Exception("No se pudo cerrar la publicación. Inténtalo de nuevo.");
            }

            //TODO: Activar flujo de reseñas para ofertas cerradas manualmente por el oferente

            return "Publicación cerrada exitosamente.";
        }
        #endregion
    }
}
