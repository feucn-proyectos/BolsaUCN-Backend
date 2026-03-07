using System.Threading.RateLimiting;
using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    public class EmailRateLimitedService : IEmailService
    {
        private readonly IEmailService _inner;

        private static readonly TokenBucketRateLimiter _limiter = new(
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 10, // Maxima cantidad de tokens en el bucket
                TokensPerPeriod = 2, // Cantidad de tokens que se añaden cada periodo
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 50,
                AutoReplenishment = true,
            }
        );

        public EmailRateLimitedService(IEmailService inner) => _inner = inner;

        private static async Task<bool> SendWithRateLimitAsync(Func<Task<bool>> send)
        {
            using var lease = await _limiter.AcquireAsync(permitCount: 1);
            if (!lease.IsAcquired)
            {
                Log.Warning("RateLimitedEmailService: rate limit queue llena, email descartado.");
                return false;
            }
            return await send();
        }

        public Task<bool> SendVerificationEmailAsync(string email, string code) =>
            SendWithRateLimitAsync(() => _inner.SendVerificationEmailAsync(email, code));

        public Task<bool> SendWelcomeEmailAsync(string email) =>
            SendWithRateLimitAsync(() => _inner.SendWelcomeEmailAsync(email));

        public Task<bool> SendResetPasswordVerificationEmailAsync(string email, string code) =>
            SendWithRateLimitAsync(() =>
                _inner.SendResetPasswordVerificationEmailAsync(email, code)
            );

        public Task<bool> SendApplicationStatusChangeEmailAsync(
            string email,
            string offerName,
            string companyName,
            string newStatus
        ) =>
            SendWithRateLimitAsync(() =>
                _inner.SendApplicationStatusChangeEmailAsync(
                    email,
                    offerName,
                    companyName,
                    newStatus
                )
            );

        public Task<bool> SendChangeEmailVerificationEmailAsync(string newEmail, string code) =>
            SendWithRateLimitAsync(() =>
                _inner.SendChangeEmailVerificationEmailAsync(newEmail, code)
            );

        public Task<bool> SendLowRatingReviewAlertAsync(ReviewDTO review) =>
            SendWithRateLimitAsync(() => _inner.SendLowRatingReviewAlertAsync(review));

        public Task<bool> SendInitialReviewsCreatedEmailAsync(
            string recipientEmail,
            Dictionary<string, string> templateData
        ) =>
            SendWithRateLimitAsync(() =>
                _inner.SendInitialReviewsCreatedEmailAsync(recipientEmail, templateData)
            );

        public Task<bool> SendPublicationClosedByAdminEmailAsync(
            string recipientEmail,
            Dictionary<string, string> templateData
        ) =>
            SendWithRateLimitAsync(() =>
                _inner.SendPublicationClosedByAdminEmailAsync(recipientEmail, templateData)
            );

        public Task<bool> SendPublicationStatusChangedEmailAsync(
            string email,
            Dictionary<string, string> templateData
        ) =>
            SendWithRateLimitAsync(() =>
                _inner.SendPublicationStatusChangedEmailAsync(email, templateData)
            );

        public Task<bool> SendDailyDigestAsync(
            string email,
            Dictionary<string, string> templateData
        ) => SendWithRateLimitAsync(() => _inner.SendDailyDigestAsync(email, templateData));

        public Task<bool> SendDailyAdminDigestAsync(
            User admin,
            Dictionary<string, string> templateData
        ) => SendWithRateLimitAsync(() => _inner.SendDailyAdminDigestAsync(admin, templateData));

        public Task<bool> SendOfferCancelledForApplicantEmailAsync(
            string email,
            Dictionary<string, string> templateData
        ) =>
            SendWithRateLimitAsync(() =>
                _inner.SendOfferCancelledForApplicantEmailAsync(email, templateData)
            );

        // No rate limiting needed for template loading
        public Task<string> LoadTemplateAsync(
            string templateName,
            Dictionary<string, string>? templateData = null
        ) => _inner.LoadTemplateAsync(templateName, templateData);
    }
}
