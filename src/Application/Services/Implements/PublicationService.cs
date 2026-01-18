using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.Services.Interfaces;
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
        private readonly IPublicationRepository _publicationRepository;
        private readonly IUserService _userService;
        private readonly IReviewService _reviewService;

        public PublicationService(
            IOfferRepository offerRepository,
            IBuySellRepository buySellRepository,
            IPublicationRepository publicationRepository,
            IReviewService reviewService,
            IUserService userService
        )
        {
            _offerRepository = offerRepository;
            _buySellRepository = buySellRepository;
            _publicationRepository = publicationRepository;
            _reviewService = reviewService;
            _userService = userService;
        }

        /// <summary>
        /// Crea una nueva oferta laboral o de voluntariado
        /// </summary>
        public async Task<string> CreateOfferAsync(CreateOfferDTO offerDTO, int userId)
        {
            // Obtener y validar el usuario actual
            User currentUser = await _userService.GetUserByIdAsync(userId);
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
                    "Usuario {userId} intentó crear publicación de compra/venta con {pendingReviewsCount} reseñas pendientes",
                    userId,
                    pendingReviewsCount
                );
                throw new InvalidOperationException(
                    "No se puede crear una oferta mientras se tengan 3 o más reseñas pendientes."
                );
            }
            bool isAdmin = currentUser.UserType == UserType.Administrador;

            // Mapeo principal DTO a Entidad
            Offer newOffer = offerDTO.Adapt<Offer>();

            // Mapeo adicional y asignaciones
            newOffer.UserId = currentUser.Id;
            newOffer.Type = Types.Oferta;

            newOffer.StatusValidation = isAdmin
                ? StatusValidation.Publicado
                : StatusValidation.EnProceso;

            newOffer.IsValidated = isAdmin;

            var createOfferResult = await _offerRepository.CreateOfferAsync(newOffer);

            Log.Information(
                "Oferta creada exitosamente. ID: {OfferId}, Título: {Title}, Usuario: {UserId}",
                createOfferResult,
                newOffer.Title,
                currentUser.Id
            );

            return $"Oferta creada exitosamente. Oferta ID: {createOfferResult}";
        }

        /// <summary>
        /// Crea una nueva publicación de compra/venta
        /// </summary>
        public async Task<string> CreateBuySellAsync(CreateBuySellDTO buySellDTO, int currentUserId)
        {
            // Obtener y validar el usuario actual
            var currentUser = await _userService.GetUserByIdAsync(currentUserId);
            var isAdmin = currentUser.UserType == UserType.Administrador;
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
            newBuySell.Type = Types.CompraVenta;

            newBuySell.StatusValidation = isAdmin
                ? StatusValidation.Publicado
                : StatusValidation.EnProceso;
            newBuySell.IsValidated = isAdmin;

            var createdBuySellResult = await _buySellRepository.CreateBuySellAsync(newBuySell);

            Log.Information(
                "Publicación de compra/venta creada exitosamente. ID: {BuySellId}, Título: {Title}, Usuario: {UserId}",
                createdBuySellResult,
                newBuySell.Title,
                currentUser.Id
            );

            return $"Publicación de compra/venta creada exitosamente. Publicación ID: {createdBuySellResult}";
        }

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
                StatusValidation = p.StatusValidation,
                UserId = p.UserId,
                Images = p.Images,
                Types = p.Type,
                IsActive = p.IsValidated,
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
                StatusValidation = p.StatusValidation,
                UserId = p.UserId,
                Images = p.Images,
                Types = p.Type,
                IsActive = p.IsValidated,
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
                StatusValidation = p.StatusValidation,
                UserId = p.UserId,
                Images = p.Images,
                Types = p.Type,
                IsActive = p.IsValidated,
            });

            return publicationsDto;
        }

        /// <summary>
        /// Approves a publication and makes it visible.
        /// </summary>
        public async Task<GenericResponse<string>> AdminApprovePublicationAsync(int publicationId)
        {
            // 1. Search for publication
            var publication = await _publicationRepository.GetByIdAsync(publicationId);

            // 2. Validation: Use Exception to trigger Middleware 404
            if (publication == null)
                throw new KeyNotFoundException("La publicación no existe.");

            // 3. Update logic
            publication.StatusValidation = StatusValidation.Publicado;
            publication.IsValidated = true;

            await _publicationRepository.UpdateAsync(publication);

            // 4. Return success only
            return new GenericResponse<string>("Publicación aprobada y publicada exitosamente.");
        }

        /// <summary>
        /// Rejects a publication with a reason and hides it.
        /// </summary>
        public async Task<GenericResponse<string>> AdminRejectPublicationAsync(
            int publicationId,
            AdminRejectDto dto
        )
        {
            var publication = await _publicationRepository.GetByIdAsync(publicationId);

            if (publication == null)
                throw new KeyNotFoundException("La publicación no existe.");

            publication.StatusValidation = StatusValidation.Rechazado;
            publication.IsValidated = false;
            publication.AdminRejectionReason = dto.Reason;

            await _publicationRepository.UpdateAsync(publication);

            return new GenericResponse<string>("Publicación rechazada. Se ha guardado el motivo.");
        }

        /// <summary>
        /// Allows a user to appeal a rejection if limits allow.
        /// </summary>
        public async Task<GenericResponse<string>> AppealPublicationAsync(
            int publicationId,
            int userId,
            UserAppealDto dto
        )
        {
            var publication = await _publicationRepository.GetByIdAsync(publicationId);

            // 1. Validate existence -> 404 Not Found
            if (publication == null)
                throw new KeyNotFoundException("La publicación no existe.");

            // 2. Validate ownership -> 401 Unauthorized (Based on JobApplicationService pattern)
            if (publication.UserId != userId)
                throw new UnauthorizedAccessException(
                    "No tienes permiso para apelar esta publicación."
                );

            // 3. Validate status -> 409 Conflict (InvalidOperationException maps to 409 in your middleware)
            if (publication.StatusValidation != StatusValidation.Rechazado)
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
            publication.StatusValidation = StatusValidation.EnProceso;
            publication.UserAppealJustification = dto.Justification;
            publication.AppealCount++;

            await _publicationRepository.UpdateAsync(publication);

            return new GenericResponse<string>(
                $"Apelación enviada exitosamente. Intento {publication.AppealCount} de {MAX_APPEALS}. Un administrador revisará tu caso."
            );
        }

        public async Task<Publication?> GetPublicationByIdAsync(
            int publicationId,
            PublicationQueryOptions options
        )
        {
            throw new NotImplementedException();
        }
    }
}
