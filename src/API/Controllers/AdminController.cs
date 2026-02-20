using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.ApplicantsForAdminDTOs;
using backend.src.Application.DTOs.UserDTOs.AdminDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace backend.src.API.Controllers
{
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly IPublicationService _publicationService;
        private readonly IOfferApplicationService _applicationService;

        public AdminController(
            IAdminService adminService,
            IPublicationService publicationService,
            IOfferApplicationService applicationService
        )
        {
            _adminService = adminService;
            _publicationService = publicationService;
            _applicationService = applicationService;
        }

        #region User Management
        /// <summary>
        /// Alterna el estado de bloqueo de un usuario.
        /// </summary>
        /// <returns>Respuesta genérica indicando el resultado de la operación.</returns>
        [HttpPatch("users/{userId}/toggle-block")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> ToggleUserBlockedStatus(int userId)
        {
            var adminId = GetUserIdFromToken();
            Log.Information(
                "Intentando alternar el estado de bloqueo del usuario con ID {UserId}",
                userId
            );
            var result = await _adminService.ToggleUserBlockedStatusAsync(adminId, userId);
            return Ok(
                new GenericResponse<bool>("Estado de bloqueo del usuario actualizado.", result)
            );
        }

        /// <summary>
        /// Obtiene todos los usuarios con parámetros de búsqueda.
        /// </summary>
        /// <param name="searchParams">Parámetros para filtrar y paginar la lista de usuarios.</param>
        /// <returns>Lista paginada y filtrada de usuarios.</returns>
        [HttpGet("users")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] UsersForAdminSearchParamsDTO searchParams
        )
        {
            var adminId = GetUserIdFromToken();
            Log.Information("Obteniendo todos los usuarios.");
            var users = await _adminService.GetAllUsersAsync(adminId, searchParams);
            return Ok(
                new GenericResponse<UsersForAdminDTO>("Usuarios obtenidos exitosamente.", users)
            );
        }

        /// <summary>
        /// Obtiene el perfil de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario cuyo perfil se desea obtener.</param>
        /// <returns>Perfil del usuario.</returns>
        [HttpGet("users/{userId}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> GetUserProfileById(int userId)
        {
            var adminId = GetUserIdFromToken();
            Log.Information("Obteniendo perfil de usuario con ID {UserId}", userId);
            var userProfile = await _adminService.GetUserProfileByIdAsync(adminId, userId);
            return Ok(
                new GenericResponse<UserProfileForAdminDTO>(
                    "Perfil de usuario obtenido exitosamente.",
                    userProfile
                )
            );
        }

        /// <summary>
        /// Elimina un administrador por su ID.
        /// </summary>
        /// <param name="userId">ID del administrador a eliminar.</param>
        /// <returns>Resultado de la operación de eliminación.</returns>
        [HttpDelete("users/{userId}")]
        [Authorize(Roles = RoleNames.SuperAdmin)]
        public async Task<IActionResult> BlockAdminById(int userId)
        {
            var superAdminId = GetUserIdFromToken();
            Log.Information("Intentando eliminar admin con ID {UserId}", userId);
            var result = await _adminService.ToggleUserBlockedStatusAsync(superAdminId, userId);
            return Ok(new GenericResponse<bool>("Admin eliminado exitosamente.", result));
        }
        #endregion
        #region Publication Management

        [HttpGet("publications")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> GetAllPublications(
            [FromQuery] PublicationsForAdminSearchParamsDTO searchParams
        )
        {
            var parsedAdminId = GetUserIdFromToken();
            var publications = await _publicationService.GetAllPublicationsForAdminAsync(
                parsedAdminId,
                searchParams
            );
            return Ok(
                new GenericResponse<PublicationsForAdminDTO>(
                    "Publicaciones aprobadas obtenidas exitosamente.",
                    publications
                )
            );
        }

        [HttpGet("publications/{publicationId}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> GetPublicationDetailsById(int publicationId)
        {
            var parsedAdminId = GetUserIdFromToken();
            var publicationDetails =
                await _publicationService.GetPublicationDetailsForAdminByIdAsync(
                    publicationId,
                    parsedAdminId
                );
            return Ok(
                new GenericResponse<PublicationDetailsForAdminDTO>(
                    "Detalles de la publicación obtenidos exitosamente.",
                    publicationDetails
                )
            );
        }

        [HttpGet("publications/{offerId}/applications")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> GetApplicantsByOfferIdForAdmin(
            int offerId,
            [FromQuery] ApplicationsForAdminSearchParamsDTO searchParams
        )
        {
            var parsedAdminId = GetUserIdFromToken();
            var applicants = await _applicationService.GetApplicationsByOfferIdForAdminAsync(
                offerId,
                parsedAdminId,
                searchParams
            );
            return Ok(
                new GenericResponse<ApplicationsForAdminDTO>(
                    "Aplicantes obtenidos exitosamente.",
                    applicants
                )
            );
        }

        [HttpPatch("publications/{publicationId}/close")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> ClosePublicationManually(
            int publicationId,
            [FromBody] ClosePublicationRequestDTO requestDTO
        )
        {
            int parsedAdminId = GetUserIdFromToken();
            var result = await _publicationService.CancelOfferManuallyAsync(
                publicationId,
                parsedAdminId,
                requestDTO
            );
            return Ok(new GenericResponse<string>("Oferta cerrada exitosamente.", result));
        }

        #endregion
        #region Review Management

        #endregion
    }
}
