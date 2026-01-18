using backend.src.Application.DTOs.UserDTOs.AdminDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace backend.src.Infrastructure.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly int _defaultPageSize;

        public UserRepository(
            AppDbContext context,
            UserManager<User> userManager,
            IConfiguration configuration
        )
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

        /// <summary>
        /// Obtiene un usuario por su correo electrónico.
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <returns>El usuario encontrado o null si no existe.</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        /// <summary>
        /// Verifica si un correo electrónico ya está registrado.
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <returns>True si el correo electrónico ya está registrado, de lo contrario false.</returns>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Verifica si un RUT ya está registrado.
        /// </summary>
        /// <param name="rut">RUT del estudiante</param>
        /// <returns>True si el RUT ya está registrado, de lo contrario false.</returns>
        public async Task<bool> ExistsByRutAsync(string rut)
        {
            return await _context.Users.AnyAsync(e => e.Rut == rut);
        }

        /// <summary>
        /// Obtiene el estado de bloqueo de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Estado de bloqueo del usuario</returns>
        public async Task<bool> GetBlockedStatusAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId && u.IsBlocked);
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="user">Usuario a crear</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>True si se creó el usuario, de lo contrario false.</returns>
        public async Task<bool> CreateUserAsync(User user, string password, string role)
        {
            Log.Information($"Creando usuario en la base de datos: {user.Email}, Rol: {role}");
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                Log.Information($"Usuario creado exitosamente: {user.Email}, UserId: {user.Id}");
                var userRole = await _userManager.AddToRoleAsync(user, role);
                if (userRole.Succeeded)
                {
                    Log.Information($"Rol {role} asignado exitosamente a usuario ID: {user.Id}");
                }
                else
                {
                    Log.Error(
                        $"Error al asignar rol {role} a usuario ID: {user.Id}. Errores: {string.Join(", ", userRole.Errors.Select(e => e.Description))}"
                    );
                }
                return userRole.Succeeded;
            }
            Log.Error(
                $"Error al crear usuario {user.Email}. Errores: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
            return false;
        }

        /// <summary>
        /// Confirma el correo electrónico de un usuario.
        /// </summary>
        /// <param name="email">Correo electrónico a confirmar</param>
        /// <returns>True si se confirmó el correo electrónico, de lo contrario false.</returns>
        public async Task<bool> ConfirmEmailAsync(string email)
        {
            Log.Information("Confirmando email en base de datos: {Email}", email);
            var result = await _context
                .Users.Where(u => u.Email == email)
                .ExecuteUpdateAsync(u => u.SetProperty(user => user.EmailConfirmed, true));

            if (result > 0)
            {
                Log.Information("Email confirmado exitosamente en base de datos: {Email}", email);
            }
            else
            {
                Log.Warning("No se pudo confirmar email en base de datos: {Email}", email);
            }
            return result > 0;
        }

        /// <summary>
        /// Verifica si la contraseña proporcionada coincide con la del usuario.
        /// </summary>
        /// <param name="user">Usuario a verificar</param>
        /// <param name="password">Contraseña a verificar</param>
        /// <returns>True si la contraseña es correcta, de lo contrario false.</returns>
        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// Actualiza la información de un usuario.
        /// </summary>
        /// <param name="user">Usuario a actualizar</param>
        /// <returns>True si la actualización fue exitosa, de lo contrario false.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<bool> UpdateAsync(User user)
        {
            Log.Information($"Actualizando informacion para el usuario Id: {user.Id}");
            var userResult = await _userManager.UpdateAsync(user);
            if (!userResult.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    userResult.Errors.Select(e => $"{e.Code}: {e.Description}")
                );
                Log.Error(
                    "Error al actualizar usuario Id: {UserId}. Identity errors: {Errors}",
                    user.Id,
                    errors
                );
                throw new InvalidOperationException(
                    $"Error al actualizar los datos del usuario: {errors}"
                );
            }
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario.
        /// </summary>
        /// <param name="user">Usuario al que se le actualizará la contraseña</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <returns>True si la contraseña se actualizó correctamente, de lo contrario false.</returns>
        public async Task<bool> UpdatePasswordAsync(User user, string newPassword)
        {
            Log.Information($"Actualizando contraseña para usuario ID: {user.Id}");
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                Log.Error(
                    "Error al eliminar la contraseña actual para usuario ID: {UserId}. Errores: {Errors}",
                    user.Id,
                    string.Join(", ", removePasswordResult.Errors.Select(e => e.Description))
                );
                return false;
            }

            var newPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!newPasswordResult.Succeeded)
            {
                Log.Error(
                    "Error al actualizar contraseña para usuario ID: {UserId}. Errores: {Errors}",
                    user.Id,
                    string.Join(", ", newPasswordResult.Errors.Select(e => e.Description))
                );
                return false;
            }

            Log.Information(
                "Contraseña actualizada exitosamente para usuario ID: {UserId}",
                user.Id
            );
            return newPasswordResult.Succeeded;
        }

        public async Task<bool> UpdateLastLoginAsync(User user)
        {
            Log.Information($"Actualizando último login para usuario ID: {user.Id}");
            user.LastLoginAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                Log.Error(
                    $"Error al actualizar último login para usuario ID: {user.Id}. Errores: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
                return false;
            }
            Log.Information($"Último login actualizado exitosamente para usuario ID: {user.Id}");
            return true;
        }

        /// <summary>
        /// Obtiene el rol del usuario.
        /// </summary>
        /// <param name="user">Usuario del cual obtener el rol</param>
        /// <returns>Rol del usuario</returns>
        public async Task<string> GetRoleAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault()!;
        }

        /// TODO: MARCADO PARA REFACTORIZACION DE PublicationController
        /// <summary>
        /// Obtiene un usuario general por su ID.
        /// </summary>
        /// <param name="id">ID del usuario a obtener</param>
        /// <returns>Usuario general</returns>
        public async Task<User> GetGeneralUserByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            return user!;
        }

        /// <summary>
        /// Obtiene un usuario por su ID con opción de tracking.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        public async Task<User?> GetByIdAsync(int userId, UserQueryOptions? options = null)
        {
            var query = _context.Users.AsQueryable();
            if (options?.TrackChanges == false)
                query = query.AsNoTracking();
            if (options?.IncludePhoto == true)
                query = query.Include(u => u.ProfilePhoto);
            if (options?.IncludeCV == true)
                query = query.Include(u => u.CV);
            if (options?.IncludeApplications == true)
                query = query.Include(u => u.Applications);
            if (options?.IncludePublications == true)
                query = query.Include(u => u.Publications);

            return await query.FirstOrDefaultAsync(u => u.Id == userId);
        }

        /// <summary>
        /// Obtiene usuarios filtrados para administración. Se salta al administrador que realiza la consulta.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación</param>
        /// <returns>Usuarios filtrados y el conteo total</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<(IEnumerable<User>, int TotalCount)> GetFilteredForAdminAsync(
            int adminId,
            SearchParamsDTO searchParams
        )
        {
            var query = _context.Users.Where(u => u.Id != adminId).AsNoTracking().AsQueryable();
            if (query == null)
            {
                Log.Warning("No se encontraron usuarios en la base de datos.");
                throw new ArgumentNullException("No se encontraron usuarios.");
            }
            // Filtro por término de búsqueda
            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                var searchTerm = searchParams.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.Email!.ToLower().Contains(searchTerm)
                    || u.Rut!.ToLower().Contains(searchTerm)
                    || u.UserName!.ToLower().Contains(searchTerm)
                );
            }
            // Filtro por tipo
            if (!string.IsNullOrEmpty(searchParams.UserType))
            {
                if (
                    Enum.TryParse(
                        searchParams.UserType,
                        ignoreCase: true,
                        out UserType userTypeEnum
                    )
                )
                {
                    query = query.Where(u => u.UserType == userTypeEnum);
                }
            }
            //Filtro por estado
            if (!string.IsNullOrEmpty(searchParams.BlockedStatus))
            {
                var status = searchParams.BlockedStatus.ToLower();
                if (status == "unblocked")
                {
                    query = query.Where(u => u.IsBlocked == false);
                }
                else if (status == "blocked")
                {
                    query = query.Where(u => u.IsBlocked == true);
                }
            }
            // Ordenamiento
            query = ApplySorting(query, searchParams.SortBy, searchParams.SortOrder);
            // Paginación
            var totalCount = await query.CountAsync();
            var pageSize = searchParams.PageSize ?? _defaultPageSize;
            var users = await query
                .Skip((searchParams.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (users, totalCount);
        }

        /// <summary>
        /// Obtiene el número de administradores en el sistema.
        /// </summary>
        /// <returns>El número de administradores activos</returns>
        public async Task<int> GetCountByTypeAsync(UserType userType)
        {
            return await _context.Users.Where(u => u.UserType == userType).CountAsync();
            //return await _context.Admins.CountAsync(a => a.GeneralUser!.Banned == false);
        }

        /// <summary>
        /// Elimina un usuario por su ID.
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <returns>True si se eliminó el usuario, de lo contrario false.</returns>
        public async Task<bool> DeleteUserAsync(User user)
        {
            Log.Information($"Intentando eliminar usuario ID: {user.Id}");
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                Log.Warning($"Usuario ID: {user.Id} no encontrado para eliminación");
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            Log.Information($"Usuario ID: {user.Id} eliminado exitosamente");
            return true;
        }

        /// <summary>
        /// Funcion helper para aplicar ordenamiento dinámico a una consulta de usuarios.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private IQueryable<User> ApplySorting(
            IQueryable<User> query,
            string? sortBy,
            string? sortOrder
        )
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(u => u.Id); // Ordenamiento por defecto es por ID
            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "asc"; // Ordenamiento por defecto es ascendente

            bool ascending = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);

            return query = sortBy.ToLower() switch
            {
                "username" => ascending
                    ? query.OrderBy(u => u.UserName)
                    : query.OrderByDescending(u => u.UserName),
                "email" => ascending
                    ? query.OrderBy(u => u.Email)
                    : query.OrderByDescending(u => u.Email),
                "rut" => ascending
                    ? query.OrderBy(u => u.Rut)
                    : query.OrderByDescending(u => u.Rut),
                "usertype" => ascending
                    ? query.OrderBy(u => u.UserType)
                    : query.OrderByDescending(u => u.UserType),
                "rating" => ascending
                    ? query.OrderBy(u => u.Rating)
                    : query.OrderByDescending(u => u.Rating),
                _ => query.OrderBy(u => u.Id), // Ordenamiento por defecto si el campo no es reconocido
            };
        }
    }
}
