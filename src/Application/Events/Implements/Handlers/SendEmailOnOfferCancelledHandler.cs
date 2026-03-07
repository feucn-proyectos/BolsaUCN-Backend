using backend.src.Application.Events.Interfaces;
using backend.src.Application.Services.Interfaces;
using Serilog;

namespace backend.src.Application.Events.Implements.Handlers
{
    public class SendEmailOnOfferCancelledHandler : IEventHandler<OfferCancelledEvent>
    {
        private readonly IEmailService _emailService;

        public SendEmailOnOfferCancelledHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task HandleAsync(
            OfferCancelledEvent evt,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                // Determinar el texto adicional basado en si fue cancelado por admin
                string cancelledByAdminText = evt.CancelledByAdmin
                    ? " por un administrador"
                    : " por el oferente";

                var templateData = new Dictionary<string, string>
                {
                    ["OFFER_TITLE"] = evt.OfferTitle,
                    ["CANCEL_REASON"] = evt.CancelReason ?? "No se proporcionó una razón.",
                    ["CANCELLED_BY_ADMIN_TEXT"] = cancelledByAdminText,
                };

                await _emailService.SendOfferCancelledForApplicantEmailAsync(
                    evt.ApplicantEmail,
                    templateData
                );

                Log.Information(
                    "Email de oferta cancelada enviado a postulante {Email} para la oferta: {OfferTitle}",
                    evt.ApplicantEmail,
                    evt.OfferTitle
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Error al enviar email de oferta cancelada para la oferta {OfferTitle} al postulante {Email}",
                    evt.OfferTitle,
                    evt.ApplicantEmail
                );
                throw;
            }
        }
    }
}
