using backend.src.Application.Events.Interfaces;
using backend.src.Application.Services.Interfaces;
using Serilog;

namespace backend.src.Application.Events.Implements.Handlers
{
    public class SendEmailOnPublicationStatusChangedHandler
        : IEventHandler<PublicationStatusChangedEvent>
    {
        private readonly IEmailService _emailService;

        public SendEmailOnPublicationStatusChangedHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task HandleAsync(
            PublicationStatusChangedEvent evt,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var templateData = new Dictionary<string, string>
                {
                    ["PUBLICATION_TITLE"] = evt.PublicationTitle!,
                    ["NEW_STATUS"] = evt.NewStatus!,
                    ["PUBLICATION_ID"] = evt.PublicationId.ToString(),
                };

                await _emailService.SendPublicationStatusChangedEmailAsync(
                    evt.OfferorEmail,
                    templateData
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Error al enviar email de cambio de estado de publicación para la publicación {PublicationId}",
                    evt.PublicationId
                );
                throw;
            }
        }
    }
}
