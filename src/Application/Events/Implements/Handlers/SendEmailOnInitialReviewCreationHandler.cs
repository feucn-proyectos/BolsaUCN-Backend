using backend.src.Application.Events.Interfaces;
using backend.src.Application.Services.Interfaces;
using Serilog;

namespace backend.src.Application.Events.Implements.Handlers
{
    public class SendEmailOnInitialReviewCreationHandler : IEventHandler<InitialReviewsCreatedEvent>
    {
        private readonly IEmailService _emailService;

        public SendEmailOnInitialReviewCreationHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task HandleAsync(
            InitialReviewsCreatedEvent evt,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var subject = $"¡Reseñas iniciales creadas para '{evt.OfferName}'!";

                var templateDataApplicants = new Dictionary<string, string>
                {
                    ["OFFER_NAME"] = evt.OfferName!,
                    ["DAYS_UNTIL_REVIEW_AUTO_CLOSE"] = evt.DaysUntilReviewAutoClose.ToString(),
                };

                var templateDataOfferor = new Dictionary<string, string>
                {
                    ["OFFER_NAME"] = evt.OfferName!,
                    ["REVIEWS_CREATED_COUNT"] = evt.ReviewsCreatedCount.ToString(),
                    ["DAYS_UNTIL_REVIEW_AUTO_CLOSE"] = evt.DaysUntilReviewAutoClose.ToString(),
                };

                foreach (var email in evt.ApplicantEmails)
                {
                    await _emailService.SendInitialReviewsCreatedEmailAsync(
                        email,
                        templateDataApplicants
                    );
                }
                // Enviar email al oferente
                await _emailService.SendInitialReviewsCreatedEmailAsync(
                    evt.OfferorEmail!,
                    templateDataOfferor
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Error al enviar email de creación de reseñas iniciales para la oferta {OfferId}",
                    evt.OfferId
                );
                throw;
            }
        }
    }
}
