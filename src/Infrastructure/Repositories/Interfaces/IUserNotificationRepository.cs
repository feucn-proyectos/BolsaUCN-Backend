using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Domain.Models;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de notificaciones, define métodos para agregar y recuperar notificaciones relacionadas con eventos de la aplicación.
    /// </summary>
    public interface IUserNotificationRepository
    {
        Task AddAsync(UserNotification notification);

        Task UpdateRangeAsync(List<UserNotification> notifications);

        Task<List<UserNotification>> GetAllUnsentAsync();
    }
}
