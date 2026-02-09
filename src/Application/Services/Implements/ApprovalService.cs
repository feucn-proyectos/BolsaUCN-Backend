using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    public class ApprovalService : IApprovalService
    {
        private readonly IPublicationService _publicationService;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public ApprovalService(
            IPublicationService publicationService,
            IPublicationRepository publicationRepository,
            IUserRepository userRepository,
            IConfiguration configuration
        )
        {
            _publicationService = publicationService;
            _publicationRepository = publicationRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

        public async Task<PublicationApprovalResultDTO> UpdatePublication(
            int userId,
            int publicationId,
            ApprovalActionDTO actionDTO
        )
        {
            User? admin =
                await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");
            bool adminResult = await _userRepository.CheckRoleAsync(userId, RoleNames.Admin);
            if (!adminResult)
            {
                Log.Error(
                    "El usuario con ID: {AdminUserId} no tiene permisos de administrador.",
                    userId
                );
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos de administrador."
                );
            }
            Publication? publication =
                await _publicationRepository.GetPublicationByIdAsync<Publication>(
                    publicationId,
                    new PublicationQueryOptions { TrackChanges = true }
                ) ?? throw new KeyNotFoundException("Publicación no encontrada.");

            if (publication.ApprovalStatus != ApprovalStatus.Pendiente)
            {
                Log.Error(
                    "La publicación con ID: {PublicationId} ya está {Status}.",
                    publicationId,
                    publication.ApprovalStatus
                );
                throw new InvalidOperationException(
                    "La publicacion ya ha sido accionada previamente."
                );
            }

            ApprovalStatus newStatus;
            if (actionDTO.Action == "publish")
            {
                newStatus = ApprovalStatus.Aceptada;
            }
            else if (actionDTO.Action == "reject")
            {
                newStatus = ApprovalStatus.Rechazada;
            }
            else
            {
                Log.Error("Acción inválida: {Action}", actionDTO.Action);
                throw new ArgumentException("Acción inválida. Debe ser 'publish' o 'reject'.");
            }

            await _publicationService.UpdatePublicationStatusAsync(publication, newStatus);
            return new PublicationApprovalResultDTO
            {
                PublicationId = publication.Id,
                RejectionReason = actionDTO.RejectionReason ?? "Razon no especificada",
            };
        }

        public async Task<PublicationsAwaitingApprovalDTO> GetPendingPublicationsAsync(
            int userId,
            PendingPublicationSearchParamsDTO searchParams
        )
        {
            // Validacion de administrador
            User? admin =
                await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");
            bool adminResult = await _userRepository.CheckRoleAsync(admin.Id, RoleNames.Admin);
            if (!adminResult)
            {
                Log.Error(
                    "El usuario con ID: {UserId} no tiene permisos de administrador.",
                    userId
                );
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos de administrador."
                );
            }

            Log.Information(
                "El usuario con ID: {UserId} está obteniendo publicaciones para validación.",
                userId
            );

            // Obtención de ofertas y compra/ventas pendientes con paginación y filtros
            var (pendingPublications, totalCount) =
                await _publicationRepository.GetAllPendingForApprovalAsync(searchParams);
            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            Log.Information(
                "El usuario con ID: {UserId} ha obtenido {Count} publicaciones para validación.",
                userId,
                pendingPublications.Count()
            );

            var publicationsDto = new PublicationsAwaitingApprovalDTO
            {
                Publications = pendingPublications.Adapt<List<PublicationAwaitingApprovalDTO>>(),
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return publicationsDto;
        }

        public async Task<PublicationDetailsForApprovalDTO> GetPublicationDetailsAsync(
            int adminId,
            int publicationId
        )
        {
            // Validacion de administrador
            bool adminExists = await _userRepository.ExistsByIdAsync(adminId);
            if (!adminExists)
            {
                Log.Error("El usuario con ID: {AdminId} no existe.", adminId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool hasRoles = await _userRepository.CheckRoleAsync(adminId, RoleNames.Admin);
            if (!hasRoles)
            {
                Log.Error(
                    "El usuario con ID: {AdminId} no tiene permisos de administrador.",
                    adminId
                );
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos de administrador."
                );
            }
            Log.Information(
                "El administrador con ID: {AdminId} está obteniendo detalles de la publicación con ID: {PublicationId} para validación.",
                adminId,
                publicationId
            );
            // Obtención de detalles de la publicación
            Publication? publication =
                await _publicationRepository.GetPublicationByIdAsync<Publication>(
                    publicationId,
                    new PublicationQueryOptions
                    {
                        IncludeImages = true,
                        IncludeUser = true,
                        TrackChanges = false,
                    }
                );
            if (publication == null)
            {
                Log.Error("La publicación con ID: {PublicationId} no existe.", publicationId);
                throw new KeyNotFoundException("Publicación no encontrada.");
            }
            else if (publication.ApprovalStatus != ApprovalStatus.Pendiente)
            {
                Log.Error(
                    "La publicación con ID: {PublicationId} no está pendiente de aprobación.",
                    publicationId
                );
                throw new InvalidOperationException(
                    "La publicación no está pendiente de aprobación."
                );
            }
            Log.Information(
                "Obteniendo detalles de la {PublicationType} con ID: {PublicationId}",
                publication.PublicationType,
                publicationId
            );
            return publication.Adapt<PublicationDetailsForApprovalDTO>();
        }
    }
}
