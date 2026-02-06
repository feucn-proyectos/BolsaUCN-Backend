using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
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
        private readonly IOfferRepository _offerRepository;
        private readonly IBuySellRepository _buySellRepository;
        private const int MAX_APPEALS = 3;
        private readonly int _defaultPageSize;
        private readonly IConfiguration _configuration;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReviewService _reviewService;

        public PublicationService(
            IOfferRepository offerRepository,
            IBuySellRepository buySellRepository,
            IPublicationRepository publicationRepository,
            IReviewService reviewService,
            IUserRepository userRepository,
            IConfiguration configuration
        )
        {
            _offerRepository = offerRepository;
            _buySellRepository = buySellRepository;
            _publicationRepository = publicationRepository;
            _reviewService = reviewService;
            _userRepository = userRepository;
            _configuration = configuration;
            _defaultPageSize = configuration.GetValue<int>("Pagination:DefaultPageSize");
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
            newOffer.ApprovalStatus = isAdmin ? ApprovalStatus.Aceptado : ApprovalStatus.EnProceso;
            newOffer.IsOpen = isAdmin;

            var createOfferResult = await _offerRepository.CreateOfferAsync(newOffer);

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
                ? ApprovalStatus.Aceptado
                : ApprovalStatus.EnProceso;
            newBuySell.IsOpen = isAdmin;

            var createdBuySellResult = await _buySellRepository.CreateBuySellAsync(newBuySell);

            Log.Information(
                "Publicación de compra/venta creada exitosamente. ID: {BuySellId}, Título: {Title}, Usuario: {UserId}",
                createdBuySellResult,
                newBuySell.Title,
                currentUser.Id
            );

            return $"Publicación de compra/venta creada exitosamente. Publicación ID: {createdBuySellResult}";
        }

        #endregion

        public async Task<IEnumerable<PublicationsDTO>> GetMyPublishedPublicationsAsync(
            string userId
        )
        {
            // 1. Llama al Repositorio para obtener los datos de la BD
            var publications = await _publicationRepository.GetPublishedPublicationsByUserIdAsync(
                userId
            );

            // 2. Mapea las entidades a DTOs
            var publicationsDto = publications.Select(p => new PublicationsDTO
            {
                IdPublication = p.Id,
                Title = p.Title,
                PublicationDate = p.CreatedAt,
                StatusValidation = p.ApprovalStatus,
                UserId = p.UserId,
                Images = p.Images,
                Types = p.PublicationType,
                IsActive = p.IsOpen,
            });

            return publicationsDto;
        }

        public async Task<IEnumerable<PublicationsDTO>> GetMyRejectedPublicationsAsync(
            string userId
        )
        {
            // 1. Llama al repositorio
            var publications = await _publicationRepository.GetRejectedPublicationsByUserIdAsync(
                userId
            );
            // 2. Mapea y devuelve el DTO
            var publicationsDto = publications.Select(p => new PublicationsDTO
            {
                IdPublication = p.Id,
                Title = p.Title,
                PublicationDate = p.CreatedAt,
                StatusValidation = p.ApprovalStatus,
                UserId = p.UserId,
                Images = p.Images,
                Types = p.PublicationType,
                IsActive = p.IsOpen,
            });

            return publicationsDto;
        }

        // --- IMPLEMENTACIÓN PENDING ("InProcess") ---
        // --- IMPLEMENTACIÓN PENDING ("InProcess") CORREGIDA ---
        public async Task<IEnumerable<PublicationsDTO>> GetMyPendingPublicationsAsync(string userId)
        {
            // 1. Llama al repositorio
            var publications = await _publicationRepository.GetPendingPublicationsByUserIdAsync(
                userId
            );

            // 2. Mapea y devuelve el DTO MANUALMENTE (Para asegurar el ID)
            var publicationsDto = publications.Select(p => new PublicationsDTO
            {
                // ✅ AQUÍ ASIGNAMOS EL ID CORRECTAMENTE
                IdPublication = p.Id,

                Title = p.Title,
                Description = p.Description, // ¡No olvides la descripción!
                PublicationDate = p.CreatedAt,
                StatusValidation = p.ApprovalStatus,
                UserId = p.UserId,
                Images = p.Images,
                Types = p.PublicationType,
                IsActive = p.IsOpen,
            });

            return publicationsDto;
        }

        #region Metodos para publicaciones en general

        //! NEEDS MORE RESEARCH - NO FLOW DOCUMENTATION FOUND
        /// <summary>
        /// Allows a user to appeal a rejection if limits allow.
        /// </summary>
        public async Task<GenericResponse<string>> AppealPublicationAsync(
            int publicationId,
            int userId,
            UserAppealDto dto
        )
        {
            var publication = await _publicationRepository.GetPublicationByIdAsync(publicationId);

            // 1. Validate existence -> 404 Not Found
            if (publication == null)
                throw new KeyNotFoundException("La publicación no existe.");

            // 2. Validate ownership -> 401 Unauthorized (Based on JobApplicationService pattern)
            if (publication.UserId != userId)
                throw new UnauthorizedAccessException(
                    "No tienes permiso para apelar esta publicación."
                );

            // 3. Validate status -> 409 Conflict (InvalidOperationException maps to 409 in your middleware)
            if (publication.ApprovalStatus != ApprovalStatus.Rechazado)
                throw new InvalidOperationException(
                    "Solo se pueden apelar publicaciones que han sido rechazadas."
                );

            // 4. Validate limits -> 409 Conflict
            if (publication.AppealCount >= MAX_APPEALS)
            {
                throw new InvalidOperationException(
                    $"Has alcanzado el límite máximo de {MAX_APPEALS} apelaciones para esta publicación."
                );
            }

            // 5. Process appeal
            publication.ApprovalStatus = ApprovalStatus.EnProceso;
            publication.UserAppealJustification = dto.Justification;
            publication.AppealCount++;

            await _publicationRepository.UpdateAsync(publication);

            return new GenericResponse<string>(
                $"Apelación enviada exitosamente. Intento {publication.AppealCount} de {MAX_APPEALS}. Un administrador revisará tu caso."
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

            var publication = await _publicationRepository.GetPublicationByIdAsync(
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
            ApprovalStatus newStatus
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
            else if (oldStatus != ApprovalStatus.EnProceso)
            {
                Log.Error(
                    "No se puede cambiar el estado de la publicación con ID {PublicationId} desde {OldStatus} a {NewStatus}.",
                    publication.Id,
                    oldStatus,
                    newStatus
                );
                throw new InvalidOperationException(
                    "Solo se pueden aprobar o rechazar publicaciones que están en estado 'EnProceso'."
                );
            }
            publication.ApprovalStatus = newStatus;
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
        #endregion
    }
}
