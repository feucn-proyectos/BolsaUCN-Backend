using backend.src.Application.DTOs.BaseResponse;
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

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
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
    }
}
