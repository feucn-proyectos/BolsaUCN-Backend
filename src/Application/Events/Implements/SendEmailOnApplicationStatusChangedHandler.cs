using backend.src.Application.Events.Interfaces;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace backend.src.Application.Events.Implements
{
    public class SendEmailOnApplicationStatusChangedHandler
        : IEventHandler<ApplicationStatusChangedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly INotificationRepository _notificationRepository;

        public SendEmailOnApplicationStatusChangedHandler(
            IEmailService emailService,
            INotificationRepository notificationRepository
        )
        {
            _emailService = emailService;
            _notificationRepository = notificationRepository;
        }

        public async Task HandleAsync(
            ApplicationStatusChangedEvent evt,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var subject = evt.NewStatus switch
                {
                    ApplicationStatus.Aceptada =>
                        $"¡Tu postulación ha sido aceptada! - {evt.OfferName}",
                    ApplicationStatus.Rechazada =>
                        $"Tu postulación ha sido rechazada - {evt.OfferName}",
                    _ => $"Estado de tu postulacion - {evt.OfferName}",
                };

                var message =
                    $"Hola {evt.ApplicantName},\n\n"
                    + $"Tu postulación a '{evt.OfferName}' en '{evt.OfferorName}' ha cambiado a '{evt.NewStatus}'.\n\n"
                    + "Puedes revisar los detalles de tu postulación en tu perfil.\n\n"
                    + "Saludos,\nEquipo de BolsaFeUCN";

                await _emailService.SendPostulationStatusChangeEmailAsync(
                    evt.ApplicantEmail,
                    evt.OfferName,
                    evt.OfferorName,
                    evt.NewStatus.ToString()
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Error al enviar email de cambio de estado para postulación {PostulationId}",
                    evt.PostulationId
                );
                throw;
            }
        }
    }
}
