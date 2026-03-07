using backend.src.Application.Events.Interfaces;
using backend.src.Application.Services.Interfaces;
using Serilog;

namespace backend.src.Application.Events.Implements.Handlers
{
    public class SendEmailOnPublicationClosedByAdminHandler
        : IEventHandler<PublicationClosedByAdminEvent>
    {
        private readonly IEmailService _emailService;

        public SendEmailOnPublicationClosedByAdminHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task HandleAsync(
            PublicationClosedByAdminEvent evt,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var templateData = new Dictionary<string, string>
                {
                    ["PUBLICATION_TITLE"] = evt.PublicationTitle!,
                    ["CLOSED_BY_ADMIN_REASON"] = evt.ClosedByAdminReason!,
                };
                await _emailService.SendPublicationClosedByAdminEmailAsync(
                    evt.OfferorEmail!,
                    templateData
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Error al enviar email de publicación cerrada por admin para la publicación {PublicationId}",
                    evt.PublicationId
                );
                throw;
            }
        }
    }
}
