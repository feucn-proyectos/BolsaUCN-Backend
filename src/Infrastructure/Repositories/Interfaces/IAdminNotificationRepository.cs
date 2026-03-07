using backend.src.Domain.Models;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    public interface IAdminNotificationRepository
    {
        Task AddAsync(AdminNotification notification);
        Task<List<AdminNotification>> GetAllUnsentAsync();
        Task UpdateRangeAsync(List<AdminNotification> notifications);
    }
}
