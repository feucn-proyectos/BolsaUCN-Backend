using backend.src.Application.DTOs.ReviewDTO;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for storing and retrieving notifications.
    /// Implementations may persist DTOs directly or map DTOs to domain entities.
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// Adds a new notification to persistent storage.
        /// </summary>
        /// <param name="notification">Notification DTO to persist.</param>
        Task AddAsync(NotificationDTO notification);

        /// <summary>
        /// Retrieves notifications for a specific user email ordered by creation date descending.
        /// </summary>
        /// <param name="email">User email to filter notifications.</param>
        Task<List<NotificationDTO>> GetByUserEmailAsync(string email);
    }
}
