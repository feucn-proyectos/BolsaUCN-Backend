using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Repositories.Interfaces;
using Bogus.Bson;
using Mapster;
using Microsoft.Extensions.Logging;

namespace backend.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio para la gestión de publicaciones de compra/venta
    /// </summary>
    public class BuySellService : IBuySellService
    {
        private readonly IBuySellRepository _buySellRepository;
        private readonly ILogger<BuySellService> _logger;
        private readonly IEmailService _emailService;

        public BuySellService(
            IBuySellRepository buySellRepository,
            ILogger<BuySellService> logger,
            IEmailService emailService
        )
        {
            _buySellRepository = buySellRepository;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<IEnumerable<BuySellSummaryDto>> GetActiveBuySellsAsync()
        {
            try
            {
                var buySells = await _buySellRepository.GetAllActiveAsync();

                var buySellDtos = buySells.Select(bs => new BuySellSummaryDto
                {
                    Id = bs.Id,
                    Title = bs.Title,
                    Category = bs.Category,
                    Price = bs.Price,
                    Location = bs.Location,
                    PublicationDate = bs.CreatedAt,
                    FirstImageUrl = bs.Images.FirstOrDefault()?.Url,
                    UserId = bs.UserId,
                    UserName = bs.User.UserName ?? "Usuario",
                    UserRating = bs.User.Rating,
                });

                _logger.LogInformation(
                    "Recuperadas {Count} publicaciones de compra/venta activas",
                    buySellDtos.Count()
                );
                return buySellDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener publicaciones de compra/venta activas");
                throw;
            }
        }

        public async Task<BuySellDetailDto?> GetBuySellDetailsAsync(int buySellId)
        {
            try
            {
                var buySell = await _buySellRepository.GetByIdAsync(buySellId);

                if (buySell == null)
                {
                    _logger.LogWarning(
                        "Publicación de compra/venta con ID {BuySellId} no encontrada",
                        buySellId
                    );
                    return null;
                }

                var buySellDto = new BuySellDetailDto
                {
                    Id = buySell.Id,
                    Title = buySell.Title,
                    Description = buySell.Description,
                    Category = buySell.Category,
                    Price = buySell.Price,
                    Location = buySell.Location,
                    ContactInfo = buySell.AdditionalContactEmail,
                    PublicationDate = buySell.CreatedAt,
                    IsActive = buySell.IsOpen,
                    ImageUrls = buySell.Images.Select(img => img.Url).ToList(),
                    UserId = buySell.UserId,
                    UserName = buySell.User.UserName ?? "Usuario",
                    UserEmail = buySell.User.Email ?? "",
                    AboutMe = buySell.User.AboutMe,
                    Rating = buySell.User.Rating,
                    statusValidation = buySell.ApprovalStatus,
                };

                _logger.LogInformation(
                    "Detalles de publicación de compra/venta {BuySellId} recuperados exitosamente",
                    buySellId
                );
                return buySellDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener detalles de la publicación de compra/venta {BuySellId}",
                    buySellId
                );
                throw;
            }
        }

        public async Task<IEnumerable<BuySellSummaryDto>> GetAllPendingBuySellsAsync()
        {
            _logger.LogInformation(
                "Obteniendo publicaciones de compra/venta pendientes de validación"
            );

            var BuySells = await _buySellRepository.GetAllPendingBuySellsAsync();
            var result = BuySells
                .Select(bs => new BuySellSummaryDto
                {
                    Id = bs.Id,
                    Title = bs.Title,
                    Category = bs.Category,
                    Price = bs.Price,
                    Location = bs.Location,
                    PublicationDate = bs.CreatedAt,
                    FirstImageUrl = bs.Images.FirstOrDefault()?.Url,
                    UserId = bs.UserId,
                    UserName = bs.User.UserName ?? "Usuario",
                    UserRating = bs.User.Rating,
                })
                .ToList();
            _logger.LogInformation(
                "Publicaciones de compra/venta pendientes obtenidas exitosamente"
            );
            return result;
        }

        public async Task<IEnumerable<BuySellBasicAdminDto>> GetPublishedBuysellsAsync()
        {
            _logger.LogInformation("Obteniendo publicaciones de compra/venta ya publicadas");
            var buysell = await _buySellRepository.GetPublishedBuySellsAsync();
            var result = buysell
                .Select(bs => new BuySellBasicAdminDto
                {
                    Id = bs.Id,
                    Title = bs.Title,
                    NameOwner = bs.User.UserName ?? "Usuario",
                    PublicationDate = bs.CreatedAt,
                    Type = bs.PublicationType,
                    IsActive = bs.IsOpen,
                })
                .ToList();
            _logger.LogInformation(
                "Publicaciones de compra/venta publicadas obtenidas exitosamente"
            );
            return result;
        }

        public async Task<BuySellDetailDto> GetBuySellDetailForOfferer(int id, string userId)
        {
            // 1. Llamar al repositorio
            // (Tu BuySellRepository.cs ya incluye User e Images en GetByIdAsync, ¡perfecto!)
            var buySell = await _buySellRepository.GetByIdAsync(id);

            // 2. Verificar si se encontró
            if (buySell == null)
            {
                throw new KeyNotFoundException($"La oferta con id {id} no fue encontrada.");
            }

            if (!int.TryParse(userId, out int parsedUserId))
            {
                throw new UnauthorizedAccessException("El ID de usuario es inválido.");
            }

            if (buySell.UserId != parsedUserId)
            {
                // Lanza 404 para no revelar que la oferta existe pero no es suya
                throw new KeyNotFoundException(
                    $"La oferta con id {id} no fue encontrada o no pertenece al usuario."
                );

                // throw new UnauthorizedAccessException("No tienes permiso para ver esta oferta.");
            }

            // 3. Mapear la entidad BuySell al DTO BuySellDetailDto
            var buySellDetailDto = buySell.Adapt<BuySellDetailDto>();

            // 4. Retornar el DTO
            return buySellDetailDto;
        }

        public async Task GetBuySellForAdminToPublish(int id)
        {
            var buySell = await _buySellRepository.GetByIdAsync(id);
            if (buySell == null)
            {
                throw new KeyNotFoundException($"La compra/venta con id {id} no fue encontrada.");
            }
            if (buySell.ApprovalStatus != ApprovalStatus.EnProceso)
            {
                throw new InvalidOperationException(
                    $"La compra/venta con ID {id} ya fue {buySell.ApprovalStatus}. No se puede publicar."
                );
            }
            buySell.IsOpen = true;
            buySell.ApprovalStatus = ApprovalStatus.Aceptado;
            await _buySellRepository.UpdateAsync(buySell);

            if (buySell.User?.Email != null)
            {
                await _emailService.SendPublicationStatusChangeEmailAsync(
                    buySell.Id,
                    buySell.User.Email,
                    buySell.Title,
                    "Publicada"
                );
            }
        }

        public async Task GetBuySellForAdminToReject(int id)
        {
            var buySell = await _buySellRepository.GetByIdAsync(id);
            if (buySell == null)
            {
                throw new KeyNotFoundException($"La compra/venta con id {id} no fue encontrada.");
            }
            if (buySell.ApprovalStatus != ApprovalStatus.EnProceso)
            {
                throw new InvalidOperationException(
                    $"La compra/venta con ID {id} ya fue {buySell.ApprovalStatus}. No se puede rechazar."
                );
            }
            buySell.IsOpen = false;
            buySell.ApprovalStatus = ApprovalStatus.Rechazado;
            await _buySellRepository.UpdateAsync(buySell);

            if (buySell.User?.Email != null)
            {
                await _emailService.SendPublicationStatusChangeEmailAsync(
                    buySell.Id,
                    buySell.User.Email,
                    buySell.Title,
                    "Rechazada"
                );
            }
        }

        public async Task ClosePublishedBuySellAsync(int buySellId)
        {
            var buySell = await _buySellRepository.GetByIdAsync(buySellId);
            if (buySell == null)
            {
                throw new KeyNotFoundException(
                    $"La compra/venta con id {buySellId} no fue encontrada."
                );
            }
            if (buySell.ApprovalStatus != ApprovalStatus.Aceptado)
            {
                throw new InvalidOperationException(
                    $"La compra/venta con ID {buySellId} está {buySell.ApprovalStatus}. No se puede cerrar."
                );
            }
            buySell.IsOpen = false;
            buySell.ApprovalStatus = ApprovalStatus.Cerrado;
            await _buySellRepository.UpdateAsync(buySell);

            {
                await _emailService.SendPublicationStatusChangeEmailAsync(
                    buySell.Id,
                    buySell.User.Email,
                    buySell.Title,
                    "Cerrada (Finalizada)" // Estado a mostrar en el email
                );
            }
        }

        public async Task ClosePublishedBuySellForOffererAsync(int buySellId, int offererUserId)
        {
            var buySell = await _buySellRepository.GetByIdAsync(buySellId);
            if (buySell == null)
            {
                throw new KeyNotFoundException(
                    $"La compra/venta con id {buySellId} no fue encontrada."
                );
            }
            // Validar propiedad: lanzar 404 para no revelar la existencia
            if (buySell.UserId != offererUserId)
            {
                throw new KeyNotFoundException(
                    $"La compra/venta con id {buySellId} no fue encontrada."
                );
            }

            if (!buySell.IsOpen)
            {
                throw new InvalidOperationException(
                    $"La compra/venta con id {buySellId} ya ha sido cerrada."
                );
            }

            if (buySell.ApprovalStatus != ApprovalStatus.Aceptado)
            {
                throw new InvalidOperationException(
                    $"La compra/venta con ID {buySellId} está {buySell.ApprovalStatus}. Solo las publicaciones activas pueden ser cerradas."
                );
            }

            buySell.IsOpen = false;
            buySell.ApprovalStatus = ApprovalStatus.Cerrado;
            await _buySellRepository.UpdateAsync(buySell);

            if (buySell.User?.Email != null)
            {
                await _emailService.SendPublicationStatusChangeEmailAsync(
                    buySell.Id,
                    buySell.User.Email,
                    buySell.Title,
                    "Cerrada (Finalizada)" // Estado a mostrar en el email
                );
            }
        }
    }
}
