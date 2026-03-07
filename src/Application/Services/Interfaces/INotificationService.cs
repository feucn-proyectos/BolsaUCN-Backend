using backend.src.Application.Events;
using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de notificaciones, que se encargará de gestionar la creación y envío de notificaciones a los usuarios y administradores
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Crea una notificación para un usuario específico
        /// </summary>
        /// <param name="recipientId">ID del usuario destinatario</param>
        /// <param name="type">Tipo de notificación</param>
        /// <param name="payload">Información adicional relevante para la notificación</param>
        Task CreateUserNotificationAsync(int recipientId, UserNotificationType type);

        /// <summary>
        /// Crea una notificación para los administradores
        /// </summary>
        /// <param name="type">Tipo de notificación</param>
        /// <param name="payload">Información adicional relevante para la notificación</param>
        Task CreateAdminNotificationAsync(AdminNotificationType type, string? payload = null);
    }
}
