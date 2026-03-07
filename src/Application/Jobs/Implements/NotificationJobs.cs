using backend.src.Application.Jobs.Interfaces;
using backend.src.Application.Services.Interfaces;
using Serilog;

namespace backend.src.Application.Jobs.Implements
{
    public class NotificationJobs : INotificationJobs
    {
        private readonly IEmailDigestService _emailDigestService;

        public NotificationJobs(IEmailDigestService emailDigestService)
        {
            _emailDigestService = emailDigestService;
        }

        public async Task SendUserDailyNotificationsAsync()
        {
            Log.Information("Iniciando proceso para enviar notificaciones diarias a los usuarios.");
            await _emailDigestService.SendDailyDigestAsync();
        }

        public async Task SendAdminDailyNotificationsAsync()
        {
            Log.Information("Iniciando proceso para enviar resúmenes diarios administrativos.");
            await _emailDigestService.SendDailyAdminDigestAsync();
        }
    }
}
