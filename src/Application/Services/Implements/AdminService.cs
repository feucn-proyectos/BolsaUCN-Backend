using backend.src.Application.DTOs.UserDTOs.AdminDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    public class AdminService : IAdminService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public AdminService(
            IUserService userService,
            ITokenService tokenService,
            IConfiguration configuration
        )
        {
            _userService = userService;
            _tokenService = tokenService;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

        /// <summary>
        /// Alterna el estado de bloqueo de un usuario dado su ID, siempre y cuando el solicitante sea un administrador.
        /// </summary>
        /// <param name="adminId">ID del administrador que realiza la acción.</param>
        /// <param name="userId">ID del usuario cuyo estado de bloqueo se desea alternar.</param>
        /// <returns>El nuevo estado de bloqueo del usuario.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> ToggleUserBlockedStatusAsync(int adminId, int userId)
        {
            //Chequeo de autobloqueo
            if (userId == adminId)
            {
                Log.Warning("Un administrador intentó alternar su propio estado de bloqueo.");
                throw new InvalidOperationException(
                    "Un administrador no puede bloquear o desbloquearse a sí mismo."
                );
            }
            Log.Information(
                "Administrador con ID {AdminId} está intentando alternar el estado de bloqueo del usuario con ID {UserId}.",
                adminId,
                userId
            );

            // Verificar que el solicitante es un administrador
            var requestingAdmin = await _userService.GetUserByIdAsync(adminId);
            var requestingRoleResult = await _userService.HasRoleAsync(
                requestingAdmin,
                RoleNames.Admin
            );
            if (!requestingRoleResult)
            {
                Log.Warning(
                    "El usuario con ID {AdminId} no tiene permisos para bloquear usuarios.",
                    adminId
                );
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos de administrador."
                );
            }

            Log.Information(
                "Buscando al usuario con ID {UserId} para alternar su estado de bloqueo.",
                userId
            );

            // Obtener el usuario objetivo
            var user = await _userService.GetUserByIdAsync(
                userId,
                new UserQueryOptions { TrackChanges = true }
            );

            var userRoleResult = await _userService.HasRoleAsync(user, RoleNames.SuperAdmin);
            if (userRoleResult) // Prevenir bloqueo de superadministradores
            {
                Log.Warning(
                    "Intento de alternar el estado de bloqueo del usuario con ID {UserId}, que es un superadministrador.",
                    userId
                );
                throw new InvalidOperationException(
                    "No se puede bloquear o desbloquear a un superadministrador."
                );
            }
            userRoleResult = await _userService.HasRoleAsync(user, RoleNames.Admin);
            if (userRoleResult) // Prevenir bloqueo de administradores si es que es el ultimo
            {
                requestingRoleResult = await _userService.HasRoleAsync(
                    requestingAdmin,
                    RoleNames.SuperAdmin
                );
                if (!requestingRoleResult)
                {
                    Log.Warning(
                        "El administrador con ID {AdminId} no tiene permisos para bloquear a otros administradores.",
                        adminId
                    );
                    throw new UnauthorizedAccessException(
                        "Solo un superadministrador puede bloquear o desbloquear a otros administradores."
                    );
                }

                int numberOfAdmins = await _userService.GetNumberOfUsersByTypeAsync(
                    UserType.Administrador
                );
                if (numberOfAdmins <= 1)
                {
                    Log.Warning("Intento de bloquear al último administrador.");
                    throw new InvalidOperationException(
                        "No se puede bloquear al último administrador."
                    );
                }
            }

            user.IsBlocked = !user.IsBlocked; // Alternar el estado de bloqueo

            var toggleResult = await _userService.UpdateUserAsync(user);
            if (toggleResult)
            {
                Log.Information(
                    "El estado de bloqueo del usuario con ID {UserId} ha sido alternado a {IsBlocked}.",
                    userId,
                    user.IsBlocked
                );
                if (user.IsBlocked)
                {
                    var revokeResult = await _tokenService.RevokeAllActiveTokensAsync(userId);
                    Log.Information(
                        revokeResult
                            ? $"Tokens activos revocados para el usuario con ID {userId} tras ser bloqueado."
                            : $"El usuario con ID {userId} no tenía tokens activos para revocar tras ser bloqueado."
                    );
                }
                return user.IsBlocked;
            }
            else
            {
                Log.Error(
                    "Error al actualizar el estado de bloqueo del usuario con ID {UserId}.",
                    userId
                );
                throw new Exception("Error al actualizar el estado de bloqueo del usuario.");
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios del sistema para un administrador.
        /// </summary>
        /// <param name="adminId">ID del administrador que realiza la solicitud</param>
        /// <returns>DTO con la lista de usuarios</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<UsersForAdminDTO> GetAllUsersAsync(
            int adminId,
            SearchParamsDTO searchParams
        )
        {
            // Verificar que el solicitante es un administrador
            var admin = await _userService.GetUserByIdAsync(adminId);
            if (admin == null)
            {
                Log.Warning("No se encontró al usuario con ID {AdminId}.", adminId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            if (admin.UserType != UserType.Administrador)
            {
                Log.Warning(
                    "El usuario con ID {AdminId} no tiene permisos de administrador.",
                    adminId
                );
                throw new UnauthorizedAccessException(
                    "El usuario no tiene permisos de administrador."
                );
            }
            // Validar y ajustar parámetros de paginación
            var (allUsers, totalCount) = await _userService.GetFilteredForAdminAsync(
                adminId,
                searchParams
            );
            if (allUsers == null)
            {
                Log.Error("Error al obtener la lista de usuarios para el administrador.");
                throw new ArgumentNullException("Error al obtener la lista de usuarios.");
            }
            var pageSize = searchParams.PageSize ?? _defaultPageSize;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var currentPage = searchParams.PageNumber;
            if (currentPage < 1 || currentPage > totalPages)
            {
                Log.Warning(
                    "Página solicitada {CurrentPage} fuera de rango. Total de páginas: {TotalPages}. Se ajusta a la página 1.",
                    currentPage,
                    totalPages
                );
                currentPage = 1;
            }
            // Aplicar paginación
            Log.Information(
                "Administrador con ID {AdminId} obtuvo {TotalCount} usuarios (página {CurrentPage} de {TotalPages}).",
                adminId,
                totalCount,
                currentPage,
                totalPages
            );
            return new UsersForAdminDTO
            {
                Users = allUsers.Adapt<List<UserForAdminDTO>>(),
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<UserProfileForAdminDTO> GetUserProfileByIdAsync(int adminId, int userId)
        {
            //TODO Revisar si hay que hacer algo especial con el administrador.

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException();

            return user.Adapt<UserProfileForAdminDTO>();
        }
    }
}
