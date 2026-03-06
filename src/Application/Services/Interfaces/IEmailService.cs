using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Domain.Models;

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
        Task<string> LoadTemplateAsync(
            string templateName,
            Dictionary<string, string>? templateData = null
        );

        /// <summary>
        /// Sends an email notifying a student that their application status has changed.
        /// </summary>
        /// <param name="email">Student email address.</param>
        /// <param name="offerName">Offer title.</param>
        /// <param name="companyName">Company name.</param>
        /// <param name="newStatus">New application status.</param>
        Task<bool> SendApplicationStatusChangeEmailAsync(
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
        /// Envia un correo notificando a los usuarios involucrados que las reseñas iniciales han sido creadas para una oferta, incluyendo detalles relevantes en el correo.
        /// </summary>
        /// <param name="recipientEmail">Correo electrónico del destinatario.</param>
        /// <param name="templateData">Datos para rellenar la plantilla del correo.</param>
        /// <returns></returns>
        Task<bool> SendInitialReviewsCreatedEmailAsync(
            string recipientEmail,
            Dictionary<string, string> templateData
        );

        /// <summary>
        /// Envia un correo notificando a los usuarios involucrados que una publicación ha sido cerrada por un administrador, incluyendo detalles relevantes en el correo.
        /// </summary>
        /// <param name="recipientEmail">Correo electrónico del destinatario.</param>
        /// <param name="templateData">Datos para rellenar la plantilla del correo.</param>
        /// <returns></returns>
        Task<bool> SendPublicationClosedByAdminEmailAsync(
            string recipientEmail,
            Dictionary<string, string> templateData
        );

        /// <summary>
        /// Envia un correo notificando a un postulante que la oferta a la que postuló ha sido cancelada.
        /// </summary>
        /// <param name="email">Correo electrónico del postulante.</param>
        /// <param name="templateData">Datos para rellenar la plantilla del correo.</param>
        /// <returns></returns>
        Task<bool> SendOfferCancelledForApplicantEmailAsync(
            string email,
            Dictionary<string, string> templateData
        );

        /// <summary>
        /// Envia un correo notificando a un usuario que el estado de su publicación ha cambiado (ej: aprobada/rechazada).
        /// </summary>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <param name="templateData">Datos para rellenar la plantilla del correo.</param>
        /// <returns></returns>
        Task<bool> SendPublicationStatusChangedEmailAsync(
            string email,
            Dictionary<string, string> templateData
        );

        /// <summary>
        /// Envia un correo con un resumen diario de notificaciones pendientes para un usuario específico.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <param name="templateData">Datos para rellenar la plantilla del correo.</param>
        /// <returns></returns>
        Task<bool> SendDailyDigestAsync(string email, Dictionary<string, string> templateData);

        /// <summary>
        /// Envia un correo con un resumen diario de notificaciones pendientes para los administradores.
        /// </summary>
        /// <param name="email">Correo electrónico del administrador.</param>
        /// <param name="templateData">Datos para rellenar la plantilla del correo.</param>
        /// <returns></returns>
        Task<bool> SendDailyAdminDigestAsync(string email, Dictionary<string, string> templateData);
    }
}
