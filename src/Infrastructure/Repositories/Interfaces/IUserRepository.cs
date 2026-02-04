using backend.src.Application.DTOs.UserDTOs.AdminDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="userId">El ID del usuario.</param>
        /// <param name="options">Opciones para la consulta del usuario.
        /// <list type="bullet">
        /// <item><description>IncludePhoto: Incluir foto del usuario.</description></item>
        /// <item><description>IncludeCV: Incluir CV del usuario.</description></item>
        /// <item><description>IncludeApplications: Incluir aplicaciones del usuario si es estudiante.</description></item>
        /// <item><description>IncludePublications: Incluir publicaciones del usuario.</description></item>
        /// <item><description>TrackChanges: Rastrear cambios en la entidad consultada.</description></item>
        /// </list>
        /// </param>
        /// <returns>El usuario correspondiente al ID proporcionado, o null si no se encuentra.</returns>
        Task<User?> GetByIdAsync(int userId, UserQueryOptions? options = null);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByIdAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByRutAsync(string rut);
        Task<bool> CreateUserAsync(User user, string password, string role);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<bool> UpdateAsync(User user);
        Task<bool> UpdatePasswordAsync(User user, string newPassword);
        Task<bool> UpdateLastLoginAsync(User user);
        Task<IList<string>> GetRolesAsync(User user);
        Task<bool> CheckRoleAsync(int userId, string role);
        Task<User> GetGeneralUserByIdAsync(int id);
        Task<bool> ConfirmEmailAsync(string email);
        Task<(IEnumerable<User>, int TotalCount)> GetUsersFilteredForAdminAsync(
            int adminId,
            UsersForAdminSearchParamsDTO searchParams
        );
        Task<int> GetCountByTypeAsync(UserType userType);
        Task<int> GetCountByRoleAsync(string role);
        Task<bool> DeleteUserAsync(User user);
    }
}
