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
    public class ValidationService : IValidationService
    {
        private readonly IPublicationService _publicationService;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public ValidationService(
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

        public async Task<ValidationResponseDTO> ValidatePublication(
            int userId,
            int publicationId,
            string action
        )
        {
            User? admin =
                await _userRepository.GetUserByIdAsync(userId)
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
                await _publicationRepository.GetPublicationByIdAsync(
                    publicationId,
                    new PublicationQueryOptions { TrackChanges = true }
                ) ?? throw new KeyNotFoundException("Publicación no encontrada.");

            if (publication.StatusValidation != StatusValidation.EnProceso)
            {
                Log.Error(
                    "La publicación con ID: {PublicationId} ya está {Status}.",
                    publicationId,
                    publication.StatusValidation
                );
                throw new InvalidOperationException(
                    "La publicacion ya ha sido accionada previamente."
                );
            }

            StatusValidation newStatus;
            if (action == "publish")
            {
                publication.IsOpen = true;
                newStatus = StatusValidation.Publicado;
            }
            else if (action == "reject")
            {
                publication.IsOpen = false;
                newStatus = StatusValidation.Rechazado;
            }
            else
            {
                Log.Error("Acción inválida: {Action}", action);
                throw new ArgumentException("Acción inválida. Debe ser 'publish' o 'reject'.");
            }

            await _publicationService.UpdatePublicationStatusAsync(publication, newStatus);
            return new ValidationResponseDTO
            {
                PublicationId = publication.Id,
                RejectionReason = action == "reject" ? "Rechazado por el administrador" : null,
            };
        }

        public async Task<PublicationsForValidationDTO> GetPublicationsForValidationAsync(
            int userId,
            SearchParamsDTO searchParamsDTO
        )
        {
            // Validacion de administrador
            User? admin =
                await _userRepository.GetUserByIdAsync(userId)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");
            bool adminResult = await _userRepository.CheckRoleAsync(userId, RoleNames.Admin);
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
                await _publicationRepository.GetAllForValidationAsync(searchParamsDTO);
            int currentPage = searchParamsDTO.PageNumber;
            int pageSize = searchParamsDTO.PageSize ?? _defaultPageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            Log.Information(
                "El usuario con ID: {UserId} ha obtenido {Count} publicaciones para validación.",
                userId,
                pendingPublications.Count()
            );

            var publicationsDto = new PublicationsForValidationDTO
            {
                Publications = pendingPublications.Adapt<List<PublicationForValidationDTO>>(),
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return publicationsDto;
        }
    }
}
