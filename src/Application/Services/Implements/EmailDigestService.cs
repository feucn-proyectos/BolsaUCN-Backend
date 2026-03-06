using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    public class EmailDigestService : IEmailDigestService
    {
        private readonly IUserNotificationRepository _notificationRepository;
        private readonly IAdminNotificationRepository _adminNotificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public EmailDigestService(
            IUserNotificationRepository notificationRepository,
            IAdminNotificationRepository adminNotificationRepository,
            IUserRepository userRepository,
            IEmailService emailService
        )
        {
            _notificationRepository = notificationRepository;
            _adminNotificationRepository = adminNotificationRepository;
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task SendDailyAdminDigestAsync()
        {
            List<AdminNotification> pendingAdminNotifications =
                await _adminNotificationRepository.GetAllUnsentAsync();
            if (pendingAdminNotifications.Count == 0)
            {
                Log.Information(
                    "No hay notificaciones administrativas pendientes para enviar en el resumen diario."
                );
                return;
            }

            Log.Information(
                "Enviando resúmenes diarios administrativos con {NotificationCount} notificaciones pendientes",
                pendingAdminNotifications.Count
            );

            IList<User> admins = await _userRepository.GetAllByRoleAsync(RoleNames.Admin);

            try
            {
                var templateData = GetDataForAdminTemplate(pendingAdminNotifications);
                foreach (var admin in admins)
                {
                    if (admin.AllowNotifications == false)
                    {
                        Log.Information(
                            "El admin con ID {AdminId} ha optado por no recibir notificaciones, se omite el envío del resumen administrativo",
                            admin.Id
                        );
                        continue;
                    }
                    await _emailService.SendDailyAdminDigestAsync(admin.Email!, templateData);
                    Log.Information(
                        "Resumen administrativo enviado a {AdminEmail} con {NotificationCount} notificaciones",
                        admin.Email,
                        pendingAdminNotifications.Count
                    );
                }

                // Marca las notificaciones como enviadas para evitar reenvíos en futuros resúmenes
                foreach (var notification in pendingAdminNotifications)
                {
                    notification.IsSent = true;
                    notification.SentAt = DateTime.UtcNow;
                }
                await _adminNotificationRepository.UpdateRangeAsync(pendingAdminNotifications);
                Log.Information(
                    "Resumen administrativo enviado con {NotificationCount} notificaciones",
                    pendingAdminNotifications.Count
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al enviar resumen diario administrativo");
            }
        }

        public async Task SendDailyDigestAsync()
        {
            List<UserNotification> pendingNotifications =
                await _notificationRepository.GetAllUnsentAsync();
            if (pendingNotifications.Count == 0)
            {
                Log.Information(
                    "No hay notificaciones pendientes para enviar en el resumen diario."
                );
                return;
            }

            var notificationsByUser = pendingNotifications
                .GroupBy(n => n.RecipientId)
                .ToDictionary(g => g.Key, g => g.ToList());

            Log.Information(
                "Enviando resúmenes diarios a {UserCount} usuarios con notificaciones pendientes",
                notificationsByUser.Count
            );

            foreach (var userGroup in notificationsByUser)
            {
                try
                {
                    await SendDigestForUserAsync(userGroup.Key, userGroup.Value);
                }
                catch (Exception ex)
                {
                    Log.Error(
                        ex,
                        "Error al enviar resumen diario para el usuario con ID {UserId}",
                        userGroup.Key
                    );
                }
            }
        }

        public async Task SendDigestForUserAsync(int userId, List<UserNotification> notifications)
        {
            User? user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                Log.Error(
                    "No se pudo enviar el resumen: usuario con ID {UserId} no encontrado",
                    userId
                );
                throw new Exception($"Usuario con ID {userId} no encontrado");
            }
            if (user.AllowNotifications == false)
            {
                Log.Information(
                    "El usuario con ID {UserId} ha optado por no recibir notificaciones, se omite el envío del resumen",
                    userId
                );
                return;
            }

            // Obtiene el numero total de notificaciones pendientes para rellenar el template
            var templateData = GetDataForTemplateByUserId(notifications);
            await _emailService.SendDailyDigestAsync(user.Email!, templateData);

            // Marca las notificaciones como enviadas para evitar reenvíos en futuros resúmenes
            foreach (var notification in notifications)
            {
                notification.IsSent = true;
                notification.SentAt = DateTime.UtcNow;
            }
            await _notificationRepository.UpdateRangeAsync(notifications);
            Log.Information(
                "Resumen enviado a {UserEmail} con {NotificationCount} notificaciones",
                user.Email,
                notifications.Count
            );
        }

        private static Dictionary<string, string> GetDataForTemplateByUserId(
            List<UserNotification> notifications
        )
        {
            int newApplicationsCount = notifications.Count(n =>
                n.Type == UserNotificationType.NuevaPostulacion
            );

            return new Dictionary<string, string>
            {
                ["NEW_APPLICATIONS"] = newApplicationsCount.ToString(),
            };
        }

        private static Dictionary<string, string> GetDataForAdminTemplate(
            List<AdminNotification> notifications
        )
        {
            int newPublications = notifications.Count(n =>
                n.Type == AdminNotificationType.NuevaPublicacion
            );
            int completedOffers = notifications.Count(n =>
                n.Type == AdminNotificationType.OfertaTerminada
            );
            int completedReviews = notifications.Count(n =>
                n.Type == AdminNotificationType.CalificacionCompletada
            );
            int newUsers = notifications.Count(n =>
                n.Type == AdminNotificationType.UsuarioRegistrado
            );
            int lowScoreWarnings = notifications.Count(n =>
                n.Type == AdminNotificationType.AvisoMalPuntaje
            );
            return new Dictionary<string, string>
            {
                ["NEW_PUBLICATIONS"] = newPublications.ToString(),
                ["COMPLETED_REVIEWS"] = completedReviews.ToString(),
                ["COMPLETED_OFFERS"] = completedOffers.ToString(),
                ["NEW_USERS"] = newUsers.ToString(),
                ["LOW_SCORE_WARNINGS"] = lowScoreWarnings.ToString(),
            };
        }
    }
}
