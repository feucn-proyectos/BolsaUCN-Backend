using backend.src.Application.DTOs.ReviewDTO;

namespace backend.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interface for the email sending service used by the application.
    /// Provides methods to send transactional and notification emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an alert email when a review rating is lower than three stars.
        /// </summary>
        /// <param name="review">Review DTO containing review details.</param>
        Task<bool> SendLowRatingReviewAlertAsync(ReviewDTO review);

        /// <summary>
        /// Sends a verification email containing a verification code.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="code">Verification code to include in the template.</param>
        Task<bool> SendVerificationEmailAsync(string email, string code);

        /// <summary>
        /// Sends a welcome email to a newly registered user.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        Task<bool> SendWelcomeEmailAsync(string email);

        /// <summary>
        /// Sends a password reset verification email.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="code">Reset verification code.</param>
        Task<bool> SendResetPasswordVerificationEmailAsync(string email, string code);

        /// <summary>
        /// Loads an email template from disk and optionally injects a code value.
        /// </summary>
        /// <param name="templateName">Template filename without extension.</param>
        /// <param name="code">Optional code to inject into the template.</param>
        /// <returns>Rendered HTML content.</returns>
        Task<string> LoadTemplateAsync(string templateName, string code);

        /// <summary>
        /// Sends an email notifying a student that their application status has changed.
        /// </summary>
        /// <param name="email">Student email address.</param>
        /// <param name="offerName">Offer title.</param>
        /// <param name="companyName">Company name.</param>
        /// <param name="newStatus">New application status.</param>
        Task<bool> SendPostulationStatusChangeEmailAsync(
            string email,
            string offerName,
            string companyName,
            string newStatus
        );

        /// <summary>
        /// Sends a verification email for changing the user's email address.
        /// </summary>
        /// <param name="newEmail">New email address to verify.</param>
        /// <param name="code">Verification code to include in the email.</param>
        /// <returns>True if the email was sent successfully; otherwise, false.</returns>
        Task<bool> SendChangeEmailVerificationEmailAsync(string newEmail, string code);

        /// <summary>
        /// Sends an email notifying the user that their publication status has changed (e.g., approved/rejected).
        /// </summary>
        /// <param name="recipientEmail">Recipient user email.</param>
        /// <param name="publicationTitle">Publication title.</param>
        /// <param name="newStatus">New status text (e.g., Publicada, Rechazada).</param>
        Task<bool> SendPublicationStatusChangeEmailAsync(
            int? publicationId,
            string recipientEmail,
            string publicationTitle,
            string newStatus
        );
    }
}
