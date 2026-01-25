using backend.src.Application.DTOs.UserDTOs.AdminDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
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
        Task<User?> GetUserByIdAsync(int userId, UserQueryOptions? options = null);
        Task<(IEnumerable<User>, int TotalCount)> GetUsersFilteredForAdminAsync(
            int adminId,
            SearchParamsDTO searchParams
        );
        Task<int> GetCountByTypeAsync(UserType userType);
        Task<int> GetCountByRoleAsync(string role);
        Task<bool> DeleteUserAsync(User user);
    }
}
