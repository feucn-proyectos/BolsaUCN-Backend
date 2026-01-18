using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Application.Events;
using backend.src.Application.Services.Interfaces;
using backend.src.Infrastructure.Repositories.Interfaces;

namespace backend.src.Application.Services.Implements
{
    /// <summary>
    /// Service responsible for creating and persisting notifications and delegating
    /// email sending when application/postulation status changes.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly INotificationRepository _notificationRepo;

        /// <summary>
        /// Constructs a new <see cref="NotificationService"/>.
        /// </summary>
        public NotificationService(
            IEmailService emailService,
            INotificationRepository notificationRepo
        )
        {
            _emailService = emailService;
            _notificationRepo = notificationRepo;
        }

        /// <summary>
        /// Handles a postulation status change event by saving a notification and sending an email.
        /// </summary>
        /// <param name="evt">Event describing the status change.</param>
        public async Task SendPostulationStatusChangeAsync(PostulationStatusChangedEvent evt)
        {
            var statusText = evt.NewStatus.ToString();

            var notification = new NotificationDTO
            {
                UserEmail = evt.StudentEmail,
                Message =
                    $"Tu postulación a '{evt.OfferName}' en '{evt.CompanyName}' ha cambiado a '{statusText}'.",
                CreatedAt = DateTime.UtcNow,
            };

            await _notificationRepo.AddAsync(notification);
            await _emailService.SendPostulationStatusChangeEmailAsync(
                evt.StudentEmail,
                evt.OfferName,
                evt.CompanyName,
                statusText
            );
        }
    }
}
