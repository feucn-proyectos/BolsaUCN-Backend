using System.Security.Claims;
using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.JobAplicationDTO;
using backend.src.Application.DTOs.OfferDTOs;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.API.Controllers
{
    /// <summary>
    /// Controlador unificado para gestión de publicaciones (Ofertas laborales y Compra/Venta)
    /// </summary>
    [Route("api/publications")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationService _publicationService;
        private readonly IUserRepository _userRepository;
        private readonly IBuySellService _buySellService;
        private readonly IOfferApplicationService _jobApplicationService;
        private readonly ILogger<PublicationController> _logger;
        private readonly IPublicationRepository _publicationRepository;

        public PublicationController(
            IPublicationService publicationService,
            IUserRepository userRepository,
            IBuySellService buySellService,
            IOfferApplicationService jobApplicationService,
            ILogger<PublicationController> logger,
            IPublicationRepository publicationRepository
        )
        {
            _publicationService = publicationService;
            _userRepository = userRepository;
            _buySellService = buySellService;
            _jobApplicationService = jobApplicationService;
            _logger = logger;
            _publicationRepository = publicationRepository;
        }

        #region Administrar ofertas y compra/venta (admin)

        /// <summary>
        /// Cierra las postulaciones de una oferta de trabajo de parte del admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("offers/{offerId}/close")]
        public async Task<IActionResult> CloseOfferForAdmin(int offerId)
        {
           throw new NotImplementedException();
        }

        /// <summary>
        /// Elimina la oferta de trabajo de parte del admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("offers/{offerId}")]
        public async Task<IActionResult> ClosePublishedOffer(int offerId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Elimina la compra y venta de parte del admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("buysells/{buySellId}")]
        public async Task<IActionResult> ClosePublishedBuySell(int buySellId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Obtener Ofertas Laborales (Público)

        /// <summary>
        /// Obtiene todas las ofertas laborales activas
        /// </summary>
        [HttpGet("offers")]
        public async Task<IActionResult> GetActiveOffers()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtiene los detalles de una oferta laboral específica
        /// SEGURIDAD: Solo estudiantes ven información completa (contacto, requisitos)
        /// Otros usuarios ven información básica sin datos sensibles
        /// </summary>
        [HttpGet("offers/{id}")]
        public async Task<IActionResult> GetOfferDetails(int id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Obtener Publicaciones de Compra/Venta (Público)

        /// <summary>
        /// Obtiene todas las publicaciones de compra/venta activas
        /// </summary>
        [HttpGet("buysells")]
        public async Task<IActionResult> GetActiveBuySells()
        {
            _logger.LogInformation(
                "GET /api/publications/buysells - Obteniendo publicaciones de compra/venta activas"
            );
            var buySells = await _buySellService.GetActiveBuySellsAsync();
            return Ok(
                new GenericResponse<IEnumerable<object>>(
                    "Publicaciones de compra/venta recuperadas exitosamente",
                    buySells
                )
            );
        }

        /// <summary>
        /// Obtiene los detalles de una publicación de compra/venta específica (público)
        /// </summary>
        [HttpGet("buysells/{id}")]
        public async Task<IActionResult> GetBuySellDetailsPublic(int id)
        {
            _logger.LogInformation(
                "GET /api/publications/buysells/{Id} - Obteniendo detalles de publicación (público)",
                id
            );
            var buySell = await _buySellService.GetBuySellDetailsAsync(id);

            if (buySell == null)
            {
                _logger.LogWarning("Publicación de compra/venta {Id} no encontrada", id);
                return NotFound(new GenericResponse<object>("Publicación no encontrada"));
            }

            return Ok(
                new GenericResponse<object>(
                    "Detalles de publicación recuperados exitosamente",
                    buySell
                )
            );
        }

        /// <summary>
        /// !DEPRECATED
        /// Obtiene los detalles de una publicación de compra/venta para validación (admin)
        /// </summary>
        [HttpGet("buysells/{id}/validation")]
        public async Task<IActionResult> GetBuySellDetailsValidation(int id)
        {
            _logger.LogInformation(
                "GET /api/publications/buysells/{Id}/validation - Obteniendo detalles de publicación para validación",
                id
            );
            var buySell = await _buySellService.GetBuySellDetailsAsync(id);

            if (buySell == null)
            {
                _logger.LogWarning("Publicación de compra/venta {Id} no encontrada", id);
                return NotFound(new GenericResponse<object>("Publicación no encontrada"));
            }

            return Ok(
                new GenericResponse<object>(
                    "Detalles de publicación recuperados exitosamente",
                    buySell
                )
            );
        }

        /// <summary>
        /// Obtiene los detalles de una publicación de compra/venta para validación (admin)
        /// </summary>
        [HttpGet("buysells/{id}/details")]
        public async Task<IActionResult> GetBuySellDetailsManage(int id)
        {
            _logger.LogInformation(
                "GET /api/publications/buysells/{Id}/validation - Obteniendo detalles de publicación para validación",
                id
            );
            var buySell = await _buySellService.GetBuySellDetailsAsync(id);

            if (buySell == null)
            {
                _logger.LogWarning("Publicación de compra/venta {Id} no encontrada", id);
                return NotFound(new GenericResponse<object>("Publicación no encontrada"));
            }

            return Ok(
                new GenericResponse<object>(
                    "Detalles de publicación recuperados exitosamente",
                    buySell
                )
            );
        }

        #endregion


        /// <summary>
        /// Allows the owner of a rejected publication to appeal the decision.
        /// </summary>
        /// <param name="id">The ID of the publication to appeal.</param>
        /// <param name="dto">The justification for the appeal.</param>
        [HttpPost("{id}/appeal")]
        [Authorize] // Cualquier usuario autenticado (que sea dueño)
        public async Task<IActionResult> AppealPublication(int id, [FromBody] UserAppealDto dto)
        {
            // 1. Obtener ID del usuario desde el Token JWT (Claims)
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "Token inválido" });

            var userId = int.Parse(userIdClaim.Value);

            // 2. Llamar al servicio (validará si es dueño, si está rechazada, límites, etc.)
            var response = await _publicationService.AppealPublicationAsync(id, userId, dto);

            return Ok(response);
        }
    }
}
