using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    public interface IEmailDigestService
    {
        Task SendDailyDigestAsync();
        Task SendDailyAdminDigestAsync();
        Task SendDigestForUserAsync(int userId, List<UserNotification> notifications);
    }
}
