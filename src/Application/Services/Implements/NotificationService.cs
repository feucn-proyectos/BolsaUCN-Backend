using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Application.Events;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Repositories.Interfaces;

namespace backend.src.Application.Services.Implements
{
    /// <summary>
    /// Implementación del servicio de notificaciones, que se encargará de gestionar la creación y envío de notificaciones a los usuarios y administradores
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IAdminNotificationRepository _adminNotificationRepository;
        private readonly IUserNotificationRepository _userNotificationRepository;

        public NotificationService(
            IUserNotificationRepository userNotificationRepository,
            IAdminNotificationRepository adminNotificationRepository
        )
        {
            _userNotificationRepository = userNotificationRepository;
            _adminNotificationRepository = adminNotificationRepository;
        }

        public async Task CreateAdminNotificationAsync(
            AdminNotificationType type,
            string? payload = null
        )
        {
            var notification = new AdminNotification
            {
                Type = type,
                Payload = payload ?? string.Empty,
                IsSent = false,
            };

            await _adminNotificationRepository.AddAsync(notification);
        }

        public async Task CreateUserNotificationAsync(int recipientId, UserNotificationType type)
        {
            var notification = new UserNotification
            {
                RecipientId = recipientId,
                Type = type,
                IsSent = false,
            };

            await _userNotificationRepository.AddAsync(notification);
        }
    }
}
