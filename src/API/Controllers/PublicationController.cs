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
        private readonly IOfferService _offerService;
        private readonly IBuySellService _buySellService;
        private readonly IJobApplicationService _jobApplicationService;
        private readonly ILogger<PublicationController> _logger;
        private readonly IPublicationRepository _publicationRepository;

        public PublicationController(
            IPublicationService publicationService,
            IUserRepository userRepository,
            IOfferService offerService,
            IBuySellService buySellService,
            IJobApplicationService jobApplicationService,
            ILogger<PublicationController> logger,
            IPublicationRepository publicationRepository
        )
        {
            _publicationService = publicationService;
            _userRepository = userRepository;
            _offerService = offerService;
            _buySellService = buySellService;
            _jobApplicationService = jobApplicationService;
            _logger = logger;
            _publicationRepository = publicationRepository;
        }

        #region Crear Publicaciones (Requiere autenticación)

        /// <summary>
        /// Crea una nueva oferta laboral o de voluntariado
        /// Cualquier usuario autenticado puede crear ofertas
        /// </summary>
        //! REPLACED
        [HttpPost("offers-old")]
        [Authorize]
        public async Task<IActionResult> CreateOffer([FromBody] CreateOfferDTO dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                _logger.LogWarning("Usuario no autenticado intentando crear oferta");
                return Unauthorized(new GenericResponse<object>("Usuario no autenticado"));
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("ID de usuario inválido: {UserId}", userIdString);
                return BadRequest(new GenericResponse<object>("ID de usuario inválido"));
            }

            var currentUser = await _userRepository.GetGeneralUserByIdAsync(userId);
            if (currentUser == null)
            {
                _logger.LogWarning("Usuario no encontrado: {UserId}", userId);
                return NotFound(new GenericResponse<object>("Usuario no encontrado"));
            }

            _logger.LogInformation("Usuario {UserId} creando oferta: {Title}", userId, dto.Title);

            try
            {
                var response = await _publicationService.CreateOfferAsync(dto, currentUser.Id);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Intento de publicación sin rol autorizado.");
                return StatusCode(403, new GenericResponse<object>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validación de negocio fallida.");
                return BadRequest(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al crear publicación de oferta.");
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al crear la publicación.")
                );
            }
        }

        /// <summary>
        /// Crea una nueva publicación de compra/venta
        /// Cualquier usuario autenticado puede crear publicaciones de compra/venta
        /// </summary>
        //! REPLACED
        [HttpPost("buysells-old")]
        [Authorize]
        public async Task<IActionResult> CreateBuySell([FromBody] CreateBuySellDTO dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                _logger.LogWarning(
                    "Usuario no autenticado intentando crear publicación de compra/venta"
                );
                return Unauthorized(new GenericResponse<object>("Usuario no autenticado"));
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("ID de usuario inválido: {UserId}", userIdString);
                return BadRequest(new GenericResponse<object>("ID de usuario inválido"));
            }

            var currentUser = await _userRepository.GetGeneralUserByIdAsync(userId);
            if (currentUser == null)
            {
                _logger.LogWarning("Usuario no encontrado: {UserId}", userId);
                return NotFound(new GenericResponse<object>("Usuario no encontrado"));
            }

            _logger.LogInformation(
                "Usuario {UserId} creando publicación de compra/venta: {Title}",
                userId,
                dto.Title
            );

            try
            {
                var response = await _publicationService.CreateBuySellAsync(dto, currentUser.Id);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Intento de publicación sin rol autorizado.");
                return StatusCode(403, new GenericResponse<object>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validación de negocio fallida.");
                return BadRequest(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al crear publicación de compra/venta.");
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al crear la publicación.")
                );
            }
        }

        #endregion

        #region Obtener ofertas y compra/venta de administración (admin)
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/my-published")]
        public async Task<IActionResult> GetAllPublicationsForAdmin()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogWarning("Usuario no autenticado intentando obtener publicaciones");
                    return Unauthorized(new GenericResponse<object>("Usuario no autenticado"));
                }

                var publicationsDto = await _publicationService.GetMyPublishedPublicationsAsync(
                    userId
                );

                return Ok(
                    new GenericResponse<IEnumerable<PublicationsDTO>>(
                        "Ofertas Publicadas obtenidas",
                        publicationsDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso no encontrado");
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida");
                return Conflict(new GenericResponse<object>(ex.Message));
            }
        }
        #endregion

        #region Administra buysells Admin

        /// <summary>
        /// Obtiene todas las ofertas pendientes de validación solo disponibles para admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("offers/pending")]
        public async Task<IActionResult> GetPendingOffersForAdmin()
        {
            var offer = await _offerService.GetPendingOffersAsync();
            if (offer == null)
            {
                return NotFound(new GenericResponse<string>("No hay ofertas pendientes", null));
            }
            return Ok(
                new GenericResponse<IEnumerable<PendingOffersForAdminDto>>(
                    "Ofertas pendientes obtenidas",
                    offer
                )
            );
        }

        /// <summary>
        /// Obtiene todas las publicaciones de compra/venta pendientes de validación solo disponibles para admin
        /// </summary>
        /// TODO: arreglar endpoint porque entrega mas informacion de la necesaria
        [Authorize(Roles = "Admin")]
        [HttpGet("buysells/pending")]
        public async Task<IActionResult> GetPendingBuySellsForAdmin()
        {
            var buySell = await _buySellService.GetAllPendingBuySellsAsync();
            if (buySell == null)
            {
                return NotFound(
                    new GenericResponse<string>(
                        "No hay publicaciones de compra/venta pendientes",
                        null
                    )
                );
            }
            return Ok(
                new GenericResponse<IEnumerable<BuySellSummaryDto>>(
                    "Publicaciones de compra/venta pendientes obtenidas",
                    buySell
                )
            );
        }

        /// <summary>
        /// Obtiene todas las ofertas publicadas solo disponibles para admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("offers/published")]
        public async Task<IActionResult> GetPublishedOffersForAdmin()
        {
            var offer = await _offerService.GetPublishedOffersAsync();
            if (offer == null)
            {
                return NotFound(new GenericResponse<string>("No hay ofertas publicadas", null));
            }
            return Ok(
                new GenericResponse<IEnumerable<OfferBasicAdminDto>>(
                    "Ofertas publicadas obtenidas",
                    offer
                )
            );
        }

        /// <summary>
        /// Obtiene todas las compra y venta publicadas solo disponibles para admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("buysells/published")]
        public async Task<IActionResult> GetPublishedBuysellsForAdmin()
        {
            var buysell = await _buySellService.GetPublishedBuysellsAsync();
            if (buysell == null)
            {
                return NotFound(
                    new GenericResponse<string>("no hay compra y ventas publicadas", null)
                );
            }
            return Ok(
                new GenericResponse<IEnumerable<BuySellBasicAdminDto>>(
                    "Compra y ventas publicadas obtenidas",
                    buysell
                )
            );
        }

        #endregion

        #region Administrar ofertas y compra/venta (admin)

        /// <summary>
        /// Obtiene los detalles de una oferta para la administracion de esta
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("offers/{offerId}/details")]
        public async Task<IActionResult> GetOfferDetailsForAdmin(int offerId)
        {
            var offer = await _offerService.GetOfferDetailsForAdminManagement(offerId);
            if (offer == null)
            {
                return NotFound(new GenericResponse<object>("No se encontro la oferta", null));
            }
            return Ok(
                new GenericResponse<OfferDetailsAdminDto>(
                    "Informacion basica de oferta recibida con exito.",
                    offer
                )
            );
        }

        /// <summary>
        /// Obtiene una lista de todos los postulantes inscritos a una oferta de trabajo
        /// </summary>
        /// TODO: agregar validacion cuando oferta ya fue cerrada
        [Authorize(Roles = "Admin")]
        [HttpGet("offers/{offerId}/applicants")]
        public async Task<IActionResult> GetApplicantsForAdmin(int offerId)
        {
            var applicants = await _jobApplicationService.GetApplicantsForAdminManagement(offerId);
            if (applicants == null)
            {
                return NotFound(new GenericResponse<object>("No se encontro la oferta", null));
            }
            return Ok(
                new GenericResponse<IEnumerable<ViewApplicantsDto>>(
                    "Lista de postulantes recibida exitosamente.",
                    applicants
                )
            );
        }

        /// <summary>
        /// Obtiene los detalles de un postulante inscrito a una oferta de trabajo
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("applications/{studentId}/details")]
        public async Task<IActionResult> GetApplicantDetailsForAdmin(int studentId)
        {
            var applicantDetail = await _jobApplicationService.GetApplicantDetailForAdmin(
                studentId
            );
            if (applicantDetail == null)
            {
                return NotFound(new GenericResponse<object>("No se encontro al postulante", null));
            }
            return Ok(
                new GenericResponse<ViewApplicantDetailAdminDto>(
                    "Informacion basica de oferta recibida con exito.",
                    applicantDetail
                )
            );
        }

        /// <summary>
        /// Cierra las postulaciones de una oferta de trabajo de parte del admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("offers/{offerId}/close")]
        public async Task<IActionResult> CloseOfferForAdmin(int offerId)
        {
            try
            {
                await _offerService.GetOfferForAdminToClose(offerId);
                return Ok(
                    new GenericResponse<object>(
                        $"Postulaciones para la oferta {offerId} cerradas con éxito por Admin.",
                        offerId
                    )
                );
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new GenericResponse<object>("No se encontro la oferta", null));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando oferta ID: {OfferId}", offerId);
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al cerrar la oferta.", null)
                );
            }
        }

        /// <summary>
        /// Acepta una compra/venta especifica SOLO ADMIN
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("buysells/{id}/publish")]
        public async Task<IActionResult> PublishBuySell(int id)
        {
            try
            {
                await _buySellService.GetBuySellForAdminToPublish(id);
                return Ok(
                    new GenericResponse<object>($"Compra/Venta {id} publicada con exito", id)
                );
            }
            catch (KeyNotFoundException)
            {
                return NotFound(
                    new GenericResponse<object>("No se encontro la Compra/Venta", null)
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando Compra/Venta ID: {Compra/Venta id}", id);
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al cerrar la compra/venta.", null)
                );
            }
        }

        /// <summary>
        /// Rechaza una compra/venta especifica SOLO ADMIN
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("buysells/{id}/reject")]
        public async Task<IActionResult> RejectBuySell(int id)
        {
            try
            {
                await _buySellService.GetBuySellForAdminToReject(id);
                return Ok(
                    new GenericResponse<object>($"Compra/Venta {id} rechazada con exito", id)
                );
            }
            catch (KeyNotFoundException)
            {
                return NotFound(
                    new GenericResponse<object>("No se encontro la Compra/Venta", null)
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando Compra/Venta ID: {Compra/VentaId}", id);
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al cerrar la Compra/Venta.", null)
                );
            }
        }

        /// <summary>
        /// Elimina la oferta de trabajo de parte del admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("offers/{offerId}")]
        public async Task<IActionResult> ClosePublishedOffer(int offerId)
        {
            try
            {
                await _offerService.ClosePublishedOfferAsync(offerId);
                return Ok(
                    new GenericResponse<object>($"Oferta {offerId} cerrada con exito", offerId)
                );
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new GenericResponse<object>("No se encontro la oferta", null));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando oferta ID: {OfferId}", offerId);
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al cerrar la oferta.", null)
                );
            }
        }

        /// <summary>
        /// Elimina la compra y venta de parte del admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("buysells/{buySellId}")]
        public async Task<IActionResult> ClosePublishedBuySell(int buySellId)
        {
            try
            {
                await _buySellService.ClosePublishedBuySellAsync(buySellId);
                return Ok(
                    new GenericResponse<object>(
                        $"Compra/Venta {buySellId} cerrada con exito",
                        buySellId
                    )
                );
            }
            catch (KeyNotFoundException)
            {
                return NotFound(
                    new GenericResponse<object>("No se encontro la Compra/Venta", null)
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando Compra/Venta ID: {BuySellId}", buySellId);
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al cerrar la Compra/Venta.", null)
                );
            }
        }
        #endregion

        #region Validar ofertas (admin)

        /// <summary>
        /// !DEPRECATED
        /// Obtiene los detalles de una oferta para su validación
        /// </summary>
        public async Task<IActionResult> GetOfferDetailsForOfferValidation(int id)
        {
            var offer = await _offerService.GetOfferDetailForOfferValidationAsync(id);
            if (offer == null)
            {
                return NotFound(new GenericResponse<object>("No se encontro al postulante", null));
            }
            return Ok(
                new GenericResponse<OfferDetailValidationDto>(
                    "Informacion basica de oferta recibida con exito.",
                    offer
                )
            );
        }

        /// <summary>
        /// !DEPRECATED
        /// Acepta una oferta laboral específica (solo admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("offers/{id}/publish")]
        public async Task<IActionResult> PublishOffer(int id)
        {
            try
            {
                await _offerService.GetOfferForAdminToPublish(id);
                return Ok(new GenericResponse<object>($"Oferta {id} publicada con exito", id));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new GenericResponse<object>("No se encontro la oferta", null));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando oferta ID: {OfferId}", id);
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al cerrar la oferta.", null)
                );
            }
        }

        /// </summary>
        /// !DEPRECATED
        /// Rechaza una oferta laboral específica (solo admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("offers/{id}/reject")]
        public async Task<IActionResult> RejectOffer(int id)
        {
            try
            {
                await _offerService.GetOfferForAdminToReject(id);
                return Ok(new GenericResponse<object>($"Oferta {id} rechazada con exito", id));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new GenericResponse<object>("No se encontro la oferta", null));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando oferta ID: {OfferId}", id);
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al cerrar la oferta.", null)
                );
            }
        }

        #endregion

        #region Obtener Ofertas Laborales (Público)

        /// <summary>
        /// Obtiene todas las ofertas laborales activas
        /// </summary>
        [HttpGet("offers")]
        public async Task<IActionResult> GetActiveOffers()
        {
            _logger.LogInformation("GET /api/publications/offers - Obteniendo ofertas activas");
            var offers = await _offerService.GetActiveOffersAsync();
            return Ok(
                new GenericResponse<IEnumerable<object>>("Ofertas recuperadas exitosamente", offers)
            );
        }

        /// <summary>
        /// Obtiene los detalles de una oferta laboral específica
        /// SEGURIDAD: Solo estudiantes ven información completa (contacto, requisitos)
        /// Otros usuarios ven información básica sin datos sensibles
        /// </summary>
        [HttpGet("offers/{id}")]
        public async Task<IActionResult> GetOfferDetails(int id)
        {
            _logger.LogInformation(
                "GET /api/publications/offers/{Id} - Obteniendo detalles de oferta",
                id
            );

            // Verificar si el usuario está autenticado y es estudiante
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isStudent = false;
            string userTypeDebug = "NO AUTENTICADO";

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                var currentUser = await _userRepository.GetGeneralUserByIdAsync(userId);
                if (currentUser != null)
                {
                    isStudent = currentUser.UserType == UserType.Estudiante;
                    userTypeDebug = currentUser.UserType.ToString();
                    _logger.LogInformation(
                        "Usuario autenticado: ID={UserId}, UserType={UserType}, EsEstudiante={IsStudent}",
                        userId,
                        userTypeDebug,
                        isStudent
                    );
                }
            }
            else
            {
                _logger.LogInformation("Usuario NO autenticado o sin token JWT válido");
            }

            var offer = await _offerService.GetOfferDetailsAsync(id);

            if (offer == null)
            {
                _logger.LogWarning("Oferta {Id} no encontrada", id);
                return NotFound(new GenericResponse<object>("Oferta no encontrada"));
            }

            // Si NO es estudiante, ocultar información sensible
            if (!isStudent)
            {
                var basicOffer = new
                {
                    Id = offer.Id,
                    Title = offer.Title,
                    CompanyName = offer.CompanyName,
                    Location = offer.Location,
                    PostDate = offer.PostDate,
                    EndDate = offer.EndDate,
                    OfferType = offer.OfferType,
                    // NO incluir: Description, Remuneration
                    Message = "⚠️ Debes ser estudiante y estar autenticado para ver descripción completa, requisitos y remuneración",
                    Debug_UserType = userTypeDebug,
                };

                _logger.LogInformation(
                    "Oferta {Id} - Usuario no-estudiante ({UserType}), devolviendo información básica SIN description/remuneration",
                    id,
                    userTypeDebug
                );

                return Ok(
                    new GenericResponse<object>(
                        "Información básica de oferta (inicia sesión como estudiante para ver detalles completos)",
                        basicOffer
                    )
                );
            }

            // Si es estudiante, devolver información completa
            _logger.LogInformation(
                "Oferta {Id} - Usuario estudiante, devolviendo información completa CON description/remuneration",
                id
            );

            return Ok(
                new GenericResponse<object>("Detalles de oferta recuperados exitosamente", offer)
            );
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

        #region Postulaciones a Ofertas (Requiere autenticación de estudiante)

        /// <summary>
        /// !DEPRECATED
        /// Permite a un estudiante postular a una oferta laboral
        /// POSTULACIÓN DIRECTA: No requiere body. Se valida CV obligatorio del perfil
        /// SEGURIDAD: Solo estudiantes pueden postular. El studentId se obtiene del token JWT
        /// </summary>
        public async Task<ActionResult<JobApplicationResponseDto>> ApplyToOffer(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// !DEPRECATED
        /// Obtiene todas las postulaciones del estudiante autenticado
        /// SEGURIDAD: Solo estudiantes pueden ver sus postulaciones. El studentId se obtiene del token JWT
        /// </summary>
        public async Task<ActionResult<IEnumerable<JobApplicationResponseDto>>> GetMyApplications()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Obtener Publicaciones por Status(Filtro para Empresa y Particular

        /// <summary>
        /// Obtiene todas las publicaciones PUBLICADAS del particular/empresa autenticado.
        /// </summary>
        [HttpGet("offerent/my-published")]
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<IActionResult> GetMyPublishedPublications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogWarning("No cuenta con autorización");
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                var publicationsDto = await _publicationService.GetMyPublishedPublicationsAsync(
                    userId
                );

                return Ok(
                    new GenericResponse<IEnumerable<PublicationsDTO>>(
                        "Ofertas Publicadas obtenidas",
                        publicationsDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso no encontrado");
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida");
                return Conflict(new GenericResponse<object>(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene todas las publicaciones PENDIENTE/ENPROCESO del particular/empresa autenticado.
        /// </summary>
        [HttpGet("offerent/my-pending")]
        [Authorize(Roles = "Offerent")]
        public async Task<IActionResult> GetMyPendingPublications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogWarning("No cuenta con autorización");
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                var publicationsDto = await _publicationService.GetMyPendingPublicationsAsync(
                    userId
                );

                return Ok(
                    new GenericResponse<IEnumerable<PublicationsDTO>>(
                        "Ofertas pendientes obtenidas",
                        publicationsDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso no encontrado");
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida");
                return Conflict(new GenericResponse<object>(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene todas las publicaciones RECHAZADAS del particular/empresa autenticado.
        /// </summary>
        [HttpGet("offerent/my-rejected")]
        [Authorize(Roles = "Offerent")]
        public async Task<IActionResult> GetMyRejectedPublications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogWarning("No cuenta con autorización");
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                var publicationsDto = await _publicationService.GetMyRejectedPublicationsAsync(
                    userId
                );

                return Ok(
                    new GenericResponse<IEnumerable<PublicationsDTO>>(
                        "Ofertas Rechazadas obtenidas",
                        publicationsDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso no encontrado");
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida");
                return Conflict(new GenericResponse<object>(ex.Message));
            }
        }

        [HttpGet("offerent/offer/{id:int}")]
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<IActionResult> GetOfferDetail(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized(new GenericResponse<object>("No autenticado."));
                }

                // 1. Llama al servicio que implementamos en el paso anterior
                var offerDetailDto = await _offerService.GetOfferDetailForOfferer(id, userId);

                // 2. Devuelve el DTO en una respuesta exitosa
                return Ok(
                    new GenericResponse<OfferDetailDto>(
                        "Detalle de la oferta obtenida",
                        offerDetailDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Oferta no encontrada con ID: {Id}", id);
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de la oferta ID: {Id}", id);
                return StatusCode(500, new GenericResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene el detalle de una publicación de Compra/Venta por su ID.
        /// </summary>
        [HttpGet("offerent/buysell/{id:int}")]
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<IActionResult> GetBuySellDetail(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized(new GenericResponse<object>("No autenticado."));
                }
                // 1. Llama al servicio correspondiente
                var buySellDetailDto = await _buySellService.GetBuySellDetailForOfferer(id, userId);

                // 2. Devuelve el DTO
                return Ok(
                    new GenericResponse<BuySellDetailDto>(
                        "Detalle de la compra y venta obtenida",
                        buySellDetailDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Publicación Compra/Venta no encontrada con ID: {Id}", id);
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener detalle de la publicación Compra/Venta ID: {Id}",
                    id
                );
                return StatusCode(500, new GenericResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Cierra una oferta de trabajo activa. Solo para el dueño de la publicación.
        /// </summary>
        /// <param name="offerId">El ID de la oferta a cerrar.</param>
        [HttpPatch("offerent/my-offer/{offerId}/close")]
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<IActionResult> CloseOfferForOfferer(int offerId)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdString, out var offererUserId))
                {
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                await _offerService.ClosePublishedOfferForOffererAsync(offerId, offererUserId);

                return Ok(
                    new GenericResponse<object>(
                        $"Oferta {offerId} cerrada con éxito por el oferente.",
                        offerId
                    )
                );
            }
            catch (KeyNotFoundException)
            {
                // Retornar 404 si la oferta no existe o no le pertenece (por el chequeo en el servicio)
                return NotFound(
                    new GenericResponse<object>(
                        "La oferta no fue encontrada o no te pertenece",
                        null
                    )
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando oferta ID: {OfferId}", offerId);
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al cerrar la oferta.", null)
                );
            }
        }

        /// <summary>
        /// Cierra una publicación de compra/venta activa. Solo para el dueño de la publicación.
        /// </summary>
        /// <param name="buySellId">El ID de la publicación de compra/venta a cerrar.</param>
        [HttpPatch("offerent/my-buysell/{buySellId}/close")]
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<IActionResult> CloseBuySellForOfferer(int buySellId)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdString, out var offererUserId))
                {
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                await _buySellService.ClosePublishedBuySellForOffererAsync(
                    buySellId,
                    offererUserId
                );

                return Ok(
                    new GenericResponse<object>(
                        $"Publicación Compra/Venta {buySellId} cerrada con éxito por el oferente.",
                        buySellId
                    )
                );
            }
            catch (KeyNotFoundException)
            {
                // Retornar 404 si la publicación no existe o no le pertenece (por el chequeo en el servicio)
                return NotFound(
                    new GenericResponse<object>(
                        "La publicación no fue encontrada o no te pertenece",
                        null
                    )
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando Compra/Venta ID: {BuySellId}", buySellId);
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al cerrar la Compra/Venta.", null)
                );
            }
        }

        #endregion

        #region Endpoints para Oferentes (Empresa/Particular)


        /// <summary>
        /// Obtiene la lista de postulantes para una oferta específica (Solo para el dueño de la oferta).
        /// </summary>
        /// <param name="offerId">El ID de la oferta</param>
        /// <returns>Una lista de los postulantes de la oferta</returns>
        [HttpGet("offerent/my-offer/{offerId}/applicants")] // <-- 1. RUTA CORREGIDA (para no chocar con la del Admin)
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<IActionResult> GetOfferApplicantsForOfferer(int offerId)
        {
            try
            {
                // 1. Obtener el ID del oferente logueado desde el Token JWT
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (
                    string.IsNullOrEmpty(userIdString)
                    || !int.TryParse(userIdString, out var offererUserId)
                )
                {
                    _logger.LogWarning(
                        "GetOfferApplicants: Token JWT inválido o sin claim de NameIdentifier."
                    );
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                _logger.LogInformation(
                    "Usuario {OffererId} solicitando postulantes para la oferta {OfferId}",
                    offererUserId,
                    offerId
                );

                // 2. Llamar al servicio
                var applicants = await _jobApplicationService.GetApplicantsForOffererAsync(
                    offerId,
                    offererUserId
                );

                return Ok(
                    new GenericResponse<IEnumerable<OffererApplicantViewDto>>(
                        "Postulantes obtenidos exitosamente",
                        applicants
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(
                    ex,
                    "GetOfferApplicants: Oferta no encontrada. OfferID: {OfferId}",
                    offerId
                );
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "GetOfferApplicants: Intento de acceso no autorizado. UserID: {UserId}, OfferID: {OfferId}",
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    offerId
                );

                // 2. ARREGLO DEL ERROR (CS1503): Usamos StatusCode 403
                return StatusCode(403, new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "GetOfferApplicants: Error interno al obtener postulantes. OfferID: {OfferId}",
                    offerId
                );
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al procesar la solicitud.")
                );
            }
        }

        [HttpGet("offerent/my-offer/{offerId}/applicants/{studentId}")] // <-- 1. RUTA CORREGIDA (para no chocar con la del Admin)
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<ActionResult<ViewApplicantUserDetailDto>> GetApplicantDetail(
            int offerId,
            int studentId
        )
        {
            // 1. Obtener offererUserId del token (como se hace en otros métodos)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (
                string.IsNullOrEmpty(userIdClaim)
                || !int.TryParse(userIdClaim, out int offererUserId)
            )
            {
                return Unauthorized(new GenericResponse<object>("No autenticado o token inválido"));
            }

            // 2. Llamar al servicio que devuelve el DTO detallado
            var applicantDetail = await _jobApplicationService.GetApplicantDetailForOfferer(
                studentId,
                offerId,
                offererUserId
            ); // Este método ya devuelve ViewApplicantUserDetailDto

            // 3. Retornar con el DTO detallado
            return Ok(
                new GenericResponse<ViewApplicantUserDetailDto>( // <-- CORRECTO
                    "Detalle del postulante obtenido exitosamente",
                    applicantDetail
                )
            );
        }

        [HttpPatch("offerent/my-offer/applicants/{status}")] // <-- 1. RUTA CORREGIDA (para no chocar con la del Admin)
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<IActionResult> AcceptApplicationOfferent(String status)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (
                string.IsNullOrEmpty(userIdClaim)
                || !int.TryParse(userIdClaim, out int offererUserId)
            )
            {
                return Unauthorized(new GenericResponse<object>("No autenticado o token inválido"));
            }

            return Ok();
        }

        /// <summary>
        /// Acepta una postulación específica (solo para el dueño de la oferta).
        /// Utiliza la lógica interna de UpdateApplicationStatusAsync.
        /// </summary>
        /// <param name="applicationId">El ID de la postulación a aceptar.</param>
        [HttpPatch("offerent/applications/{applicationId}/accept")]
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<IActionResult> AcceptApplication(int applicationId)
        {
            return await UpdateApplicationStatusInternal(
                applicationId,
                ApplicationStatus.Aceptada,
                "aceptada"
            );
        }

        /// <summary>
        /// Rechaza una postulación específica (solo para el dueño de la oferta).
        /// Utiliza la lógica interna de UpdateApplicationStatusAsync.
        /// </summary>
        /// <param name="applicationId">El ID de la postulación a rechazar.</param>
        [HttpPatch("offerent/applications/{applicationId}/reject")]
        [Authorize(Roles = "Offerent,Admin")]
        public async Task<IActionResult> RejectApplication(int applicationId)
        {
            return await UpdateApplicationStatusInternal(
                applicationId,
                ApplicationStatus.Rechazada,
                "rechazada"
            );
        }

        /// <summary>
        /// Lógica interna para actualizar el estado de una postulación.
        /// Llama al JobApplicationService.UpdateApplicationStatusAsync.
        /// </summary>
        private async Task<IActionResult> UpdateApplicationStatusInternal(
            int applicationId,
            ApplicationStatus newStatus,
            string actionText
        )
        {
            try
            {
                // 1. Obtener el ID del oferente logueado desde el Token JWT
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (
                    string.IsNullOrEmpty(userIdString)
                    || !int.TryParse(userIdString, out var offererUserId)
                )
                {
                    _logger.LogWarning(
                        "UpdateApplicationStatusInternal: Token JWT inválido o sin claim de NameIdentifier."
                    );
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                _logger.LogInformation(
                    "Usuario {OffererId} intentando actualizar postulación {ApplicationId} a {NewStatus}",
                    offererUserId,
                    applicationId,
                    newStatus
                );

                // 2. Llamar al servicio para actualizar el estado.
                // Esta llamada está protegida en el servicio para que solo el dueño de la oferta pueda modificarla.
                var result = await _jobApplicationService.UpdateApplicationStatusAsync(
                    applicationId,
                    newStatus,
                    offererUserId
                );

                if (result)
                {
                    return Ok(
                        new GenericResponse<object>(
                            $"Postulación {applicationId} {actionText} exitosamente.",
                            applicationId
                        )
                    );
                }

                _logger.LogWarning(
                    "Fallo al actualizar postulación {ApplicationId} a {NewStatus} (Servicio retornó false)",
                    applicationId,
                    newStatus
                );
                return BadRequest(
                    new GenericResponse<object>(
                        $"No se pudo actualizar la postulación a '{newStatus}'"
                    )
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "UpdateApplicationStatusInternal: Intento de acceso no autorizado. {Message}",
                    ex.Message
                );
                return StatusCode(403, new GenericResponse<object>(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(
                    ex,
                    "UpdateApplicationStatusInternal: Recurso no encontrado. {Message}",
                    ex.Message
                );
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "UpdateApplicationStatusInternal: Argumento inválido. {Message}",
                    ex.Message
                );
                return BadRequest(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "UpdateApplicationStatusInternal: Error interno al actualizar postulación {ApplicationId}",
                    applicationId
                );
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al procesar la solicitud.")
                );
            }
        }

        #endregion

        #region Filtros para Estudiante(publicaciones como Offers o BuyandSell)


        /// <summary>
        /// Obtiene todas las publicaciones PUBLICADAS del particular/empresa autenticado.
        /// </summary>
        [HttpGet("Students/my-published")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> StudentGetMyPublishedPublications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogWarning("No cuenta con autorización");
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                var publicationsDto = await _publicationService.GetMyPublishedPublicationsAsync(
                    userId
                );

                return Ok(
                    new GenericResponse<IEnumerable<PublicationsDTO>>(
                        "Ofertas Publicadas obtenidas",
                        publicationsDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso no encontrado");
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida");
                return Conflict(new GenericResponse<object>(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene todas las publicaciones PENDIENTE/ENPROCESO del particular/empresa autenticado.
        /// </summary>
        [HttpGet("Students/my-pending")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> StudentGetMyPendingPublications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogWarning("No cuenta con autorización");
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                var publicationsDto = await _publicationService.GetMyPendingPublicationsAsync(
                    userId
                );

                return Ok(
                    new GenericResponse<IEnumerable<PublicationsDTO>>(
                        "Ofertas pendientes obtenidas",
                        publicationsDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso no encontrado");
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida");
                return Conflict(new GenericResponse<object>(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene todas las publicaciones RECHAZADAS del particular/empresa autenticado.
        /// </summary>
        [HttpGet("Students/my-rejected")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> StudentGetMyRejectedPublications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogWarning("No cuenta con autorización");
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                var publicationsDto = await _publicationService.GetMyRejectedPublicationsAsync(
                    userId
                );

                return Ok(
                    new GenericResponse<IEnumerable<PublicationsDTO>>(
                        "Ofertas Rechazadas obtenidas",
                        publicationsDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso no encontrado");
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida");
                return Conflict(new GenericResponse<object>(ex.Message));
            }
        }

        [HttpGet("Students/offer/{id:int}")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> StudentGetOfferDetail(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized(new GenericResponse<object>("No autenticado."));
                }

                // 1. Llama al servicio que implementamos en el paso anterior
                var offerDetailDto = await _offerService.GetOfferDetailForOfferer(id, userId);

                // 2. Devuelve el DTO en una respuesta exitosa
                return Ok(
                    new GenericResponse<OfferDetailDto>(
                        "Detalle de la oferta obtenida",
                        offerDetailDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Oferta no encontrada con ID: {Id}", id);
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de la oferta ID: {Id}", id);
                return StatusCode(500, new GenericResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene el detalle de una publicación de Compra/Venta por su ID.
        /// </summary>
        [HttpGet("Students/buysell/{id:int}")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> StudentGetBuySellDetail(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized(new GenericResponse<object>("No autenticado."));
                }
                // 1. Llama al servicio correspondiente
                var buySellDetailDto = await _buySellService.GetBuySellDetailForOfferer(id, userId);

                // 2. Devuelve el DTO
                return Ok(
                    new GenericResponse<BuySellDetailDto>(
                        "Detalle de la compra y venta obtenida",
                        buySellDetailDto
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Publicación Compra/Venta no encontrada con ID: {Id}", id);
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener detalle de la publicación Compra/Venta ID: {Id}",
                    id
                );
                return StatusCode(500, new GenericResponse<object>("Error interno del servidor"));
            }
        }
        #endregion

        #region Endpoints para Estudiantes

        /// <summary>
        /// Obtiene la lista de postulantes para una oferta específica (Solo para el dueño de la oferta).
        /// </summary>
        /// <param name="offerId">El ID de la oferta</param>
        /// <returns>Una lista de los postulantes de la oferta</returns>
        [HttpGet("Students/my-offer/{offerId}/applicants")] // <-- 1. RUTA CORREGIDA (para no chocar con la del Admin)
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> GetOfferApplicantsForStudent(int offerId)
        {
            try
            {
                // 1. Obtener el ID del oferente logueado desde el Token JWT
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (
                    string.IsNullOrEmpty(userIdString)
                    || !int.TryParse(userIdString, out var offererUserId)
                )
                {
                    _logger.LogWarning(
                        "GetOfferApplicants: Token JWT inválido o sin claim de NameIdentifier."
                    );
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                _logger.LogInformation(
                    "Usuario {OffererId} solicitando postulantes para la oferta {OfferId}",
                    offererUserId,
                    offerId
                );

                // 2. Llamar al servicio
                var applicants = await _jobApplicationService.GetApplicantsForOffererAsync(
                    offerId,
                    offererUserId
                );

                return Ok(
                    new GenericResponse<IEnumerable<OffererApplicantViewDto>>(
                        "Postulantes obtenidos exitosamente",
                        applicants
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(
                    ex,
                    "GetOfferApplicants: Oferta no encontrada. OfferID: {OfferId}",
                    offerId
                );
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "GetOfferApplicants: Intento de acceso no autorizado. UserID: {UserId}, OfferID: {OfferId}",
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    offerId
                );

                // 2. ARREGLO DEL ERROR (CS1503): Usamos StatusCode 403
                return StatusCode(403, new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "GetOfferApplicants: Error interno al obtener postulantes. OfferID: {OfferId}",
                    offerId
                );
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al procesar la solicitud.")
                );
            }
        }

        [HttpGet("Students/my-offer/{offerId}/applicants/{studentId}")] // <-- 1. RUTA CORREGIDA (para no chocar con la del Admin)
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult<ViewApplicantUserDetailDto>> StudentGetApplicantDetail(
            int offerId,
            int studentId
        )
        {
            // 1. Obtener offererUserId del token (como se hace en otros métodos)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (
                string.IsNullOrEmpty(userIdClaim)
                || !int.TryParse(userIdClaim, out int offererUserId)
            )
            {
                return Unauthorized(new GenericResponse<object>("No autenticado o token inválido"));
            }

            // 2. Llamar al servicio que devuelve el DTO detallado
            var applicantDetail = await _jobApplicationService.GetApplicantDetailForOfferer(
                studentId,
                offerId,
                offererUserId
            ); // Este método ya devuelve ViewApplicantUserDetailDto

            // 3. Retornar con el DTO detallado
            return Ok(
                new GenericResponse<ViewApplicantUserDetailDto>( // <-- CORRECTO
                    "Detalle del postulante obtenido exitosamente",
                    applicantDetail
                )
            );
        }

        [HttpPatch("Students/my-offer/applicants/{status}")] // <-- 1. RUTA CORREGIDA (para no chocar con la del Admin)
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> StudentAcceptApplication(String status)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (
                string.IsNullOrEmpty(userIdClaim)
                || !int.TryParse(userIdClaim, out int offererUserId)
            )
            {
                return Unauthorized(new GenericResponse<object>("No autenticado o token inválido"));
            }

            return Ok();
        }

        /// <summary>
        /// Acepta una postulación específica (solo para el dueño de la oferta).
        /// Utiliza la lógica interna de UpdateApplicationStatusAsync.
        /// </summary>
        /// <param name="applicationId">El ID de la postulación a aceptar.</param>
        [HttpPatch("Students/applications/{applicationId}/accept")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> StudentAcceptApplication(int applicationId)
        {
            return await StudentUpdateApplicationStatusInternal(
                applicationId,
                ApplicationStatus.Aceptada,
                "aceptada"
            );
        }

        /// <summary>
        /// Rechaza una postulación específica (solo para el dueño de la oferta).
        /// Utiliza la lógica interna de UpdateApplicationStatusAsync.
        /// </summary>
        /// <param name="applicationId">El ID de la postulación a rechazar.</param>
        [HttpPatch("Student/applications/{applicationId}/reject")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> StudentRejectApplication(int applicationId)
        {
            return await StudentUpdateApplicationStatusInternal(
                applicationId,
                ApplicationStatus.Rechazada,
                "rechazada"
            );
        }

        /// <summary>
        /// Lógica interna para actualizar el estado de una postulación.
        /// Llama al JobApplicationService.UpdateApplicationStatusAsync.
        /// </summary>
        private async Task<IActionResult> StudentUpdateApplicationStatusInternal(
            int applicationId,
            ApplicationStatus newStatus,
            string actionText
        )
        {
            try
            {
                // 1. Obtener el ID del oferente logueado desde el Token JWT
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (
                    string.IsNullOrEmpty(userIdString)
                    || !int.TryParse(userIdString, out var StudentUserId)
                )
                {
                    _logger.LogWarning(
                        "UpdateApplicationStatusInternal: Token JWT inválido o sin claim de NameIdentifier."
                    );
                    return Unauthorized(
                        new GenericResponse<object>("No autenticado o token inválido")
                    );
                }

                _logger.LogInformation(
                    "Usuario {OffererId} intentando actualizar postulación {ApplicationId} a {NewStatus}",
                    StudentUserId,
                    applicationId,
                    newStatus
                );

                // 2. Llamar al servicio para actualizar el estado.
                // Esta llamada está protegida en el servicio para que solo el dueño de la oferta pueda modificarla.
                var result = await _jobApplicationService.UpdateApplicationStatusAsync(
                    applicationId,
                    newStatus,
                    StudentUserId
                );

                if (result)
                {
                    return Ok(
                        new GenericResponse<object>(
                            $"Postulación {applicationId} {actionText} exitosamente.",
                            applicationId
                        )
                    );
                }

                _logger.LogWarning(
                    "Fallo al actualizar postulación {ApplicationId} a {NewStatus} (Servicio retornó false)",
                    applicationId,
                    newStatus
                );
                return BadRequest(
                    new GenericResponse<object>(
                        $"No se pudo actualizar la postulación a '{newStatus}'"
                    )
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "UpdateApplicationStatusInternal: Intento de acceso no autorizado. {Message}",
                    ex.Message
                );
                return StatusCode(403, new GenericResponse<object>(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(
                    ex,
                    "UpdateApplicationStatusInternal: Recurso no encontrado. {Message}",
                    ex.Message
                );
                return NotFound(new GenericResponse<object>(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "UpdateApplicationStatusInternal: Argumento inválido. {Message}",
                    ex.Message
                );
                return BadRequest(new GenericResponse<object>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "UpdateApplicationStatusInternal: Error interno al actualizar postulación {ApplicationId}",
                    applicationId
                );
                return StatusCode(
                    500,
                    new GenericResponse<object>("Error interno al procesar la solicitud.")
                );
            }
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
