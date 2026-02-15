using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Application.Services.Interfaces;
using Resend;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    /// <summary>
    /// Default implementation of <see cref="IEmailService"/>.
    /// Responsible for loading templates and sending transactional emails.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public EmailService(
            IResend resend,
            IConfiguration configuration,
            IWebHostEnvironment environment
        )
        {
            _resend = resend;
            _configuration = configuration;
            _environment = environment;
        }

        // --------------------------------------------------------------------
        // 1. EMAIL DE VERIFICACIÓN
        // --------------------------------------------------------------------
        /// <summary>
        /// Sends an account verification email containing a verification code.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="code">Verification code to include in the template.</param>
        public async Task<bool> SendVerificationEmailAsync(string email, string code)
        {
            try
            {
                Log.Information("Iniciando envío de email de verificación a: {Email}", email);
                var htmlBody = await LoadTemplateAsync("VerificationEmail", code);

                var message = new EmailMessage
                {
                    To = email,
                    From = _configuration["EmailConfiguration:From"]!,
                    Subject = _configuration["EmailConfiguration:VerificationSubject"]!,
                    HtmlBody = htmlBody,
                };

                var result = await _resend.EmailSendAsync(message);

                if (!result.Success)
                {
                    Log.Error("El envío del email de verificación falló para: {Email}", email);
                    return false;
                }

                Log.Information("Email de verificación enviado exitosamente a: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al enviar email de verificación a: {Email}", email);
                return false;
            }
        }

        // --------------------------------------------------------------------
        // 2. EMAIL RESETEO DE CONTRASEÑA
        // --------------------------------------------------------------------
        /// <summary>
        /// Sends a password reset email with a verification code to the specified address.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="code">Reset verification code.</param>
        public async Task<bool> SendResetPasswordVerificationEmailAsync(string email, string code)
        {
            try
            {
                Log.Information("Iniciando envío de email de restablecimiento a: {Email}", email);
                var htmlBody = await LoadTemplateAsync("PasswordResetEmail", code);

                var message = new EmailMessage
                {
                    To = email,
                    From = _configuration["EmailConfiguration:From"]!,
                    Subject = _configuration["EmailConfiguration:PasswordResetSubject"]!,
                    HtmlBody = htmlBody,
                };

                await _resend.EmailSendAsync(message);

                Log.Information("Email de restablecimiento enviado exitosamente a: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al enviar email de restablecimiento a: {Email}", email);
                return false;
            }
        }

        // --------------------------------------------------------------------
        // 3. EMAIL DE BIENVENIDA
        // --------------------------------------------------------------------
        /// <summary>
        /// Sends a welcome email to a newly registered user.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        public async Task<bool> SendWelcomeEmailAsync(string email)
        {
            try
            {
                Log.Information("Iniciando envío de email de bienvenida a: {Email}", email);
                var htmlBody = await LoadTemplateAsync("WelcomeEmail", null);

                var message = new EmailMessage
                {
                    From = _configuration["EmailConfiguration:From"]!,
                    To = email,
                    Subject = _configuration["EmailConfiguration:WelcomeSubject"]!,
                    HtmlBody = htmlBody,
                };

                await _resend.EmailSendAsync(message);

                Log.Information("Email de bienvenida enviado exitosamente a: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al enviar email de bienvenida a: {Email}", email);
                return false;
            }
        }

        // --------------------------------------------------------------------
        // 4. TEMPLATE LOADER GENERAL
        // --------------------------------------------------------------------
        /// <summary>
        /// Loads an email template file from disk and optionally injects a code placeholder.
        /// </summary>
        /// <param name="templateName">Template file name without extension.</param>
        /// <param name="code">Optional code to replace in the template.</param>
        /// <returns>Rendered HTML content of the template.</returns>
        public async Task<string> LoadTemplateAsync(string templateName, string? code)
        {
            try
            {
                var templatePath = Path.Combine(
                    _environment.ContentRootPath,
                    "src",
                    "Application",
                    "Templates",
                    "Emails",
                    $"{templateName}.html"
                );

                Log.Debug(
                    "Cargando template de email: {TemplateName} desde {Path}",
                    templateName,
                    templatePath
                );

                var htmlContent = await File.ReadAllTextAsync(templatePath);

                if (code != null)
                    htmlContent = htmlContent.Replace("{{CODE}}", code);

                return htmlContent;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al cargar template de email: {TemplateName}", templateName);
                throw;
            }
        }

        // --------------------------------------------------------------------
        // 5. EMAIL CAMBIO DE ESTADO DE POSTULACIÓN
        // --------------------------------------------------------------------
        /// <summary>
        /// Sends an email notifying the student that their application status has changed.
        /// </summary>
        /// <param name="email">Recipient student email.</param>
        /// <param name="offerName">Offer title.</param>
        /// <param name="companyName">Company name.</param>
        /// <param name="newStatus">New status text.</param>
        public async Task<bool> SendPostulationStatusChangeEmailAsync(
            string email,
            string offerName,
            string companyName,
            string newStatus
        )
        {
            try
            {
                Log.Information("Enviando email de cambio de estado a {Email}", email);

                var htmlBody = await LoadPostulationStatusTemplateAsync(
                    offerName,
                    companyName,
                    newStatus
                );

                var message = new EmailMessage
                {
                    To = email,
                    From = _configuration["EmailConfiguration:From"]!,
                    Subject = "Actualización en tu postulación",
                    HtmlBody = htmlBody,
                };

                var result = await _resend.EmailSendAsync(message);

                if (!result.Success)
                {
                    Log.Error("Error al enviar correo de cambio de estado a {Email}", email);
                    return false;
                }

                Log.Information("Correo de cambio de estado enviado exitosamente a {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error enviando correo de cambio de estado a {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendChangeEmailVerificationEmailAsync(string email, string code)
        {
            try
            {
                Log.Information("Iniciando envío de email de verificación a: {Email}", email);
                var htmlBody = await LoadTemplateAsync("EmailChangeVerification", code);

                var message = new EmailMessage
                {
                    To = email,
                    From = _configuration["EmailConfiguration:From"]!,
                    Subject = _configuration["EmailConfiguration:VerificationSubject"]!,
                    HtmlBody = htmlBody,
                };

                var result = await _resend.EmailSendAsync(message);

                if (!result.Success)
                {
                    Log.Error("El envío del email de verificación falló para: {Email}", email);
                    return false;
                }

                Log.Information("Email de verificación enviado exitosamente a: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al enviar email de verificación a: {Email}", email);
                return false;
            }
        }

        private async Task<string> LoadPostulationStatusTemplateAsync(
            string offerName,
            string companyName,
            string newStatus
        )
        {
            try
            {
                var templatePath = Path.Combine(
                    _environment.ContentRootPath,
                    "src",
                    "Application",
                    "Templates",
                    "Emails",
                    "PostulationStatusChanged.html"
                );

                var html = await File.ReadAllTextAsync(templatePath);

                html = html.Replace("{{OFFER_NAME}}", offerName);
                html = html.Replace("{{COMPANY_NAME}}", companyName);
                html = html.Replace("{{NEW_STATUS}}", newStatus);

                return html;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error cargando template PostulationStatusChanged");
                throw;
            }
        }

        public async Task<bool> SendPublicationStatusChangeEmailAsync(
            int? publicationId,
            string recipientEmail,
            string publicationTitle,
            string newStatus
        )
        {
            try
            {
                Log.Information(
                    "Enviando email de cambio de estado de publicación a {Email}",
                    recipientEmail
                );

                bool isRejected = newStatus.Equals("Rechazada", StringComparison.OrdinalIgnoreCase);

                var htmlBody = await LoadPublicationStatusTemplateAsync(
                    publicationId,
                    publicationTitle,
                    newStatus
                );

                var message = new EmailMessage
                {
                    To = recipientEmail,
                    From = _configuration["EmailConfiguration:From"]!,
                    Subject = isRejected
                        ? "Tu publicación fue rechazada"
                        : "Actualización en tu publicación",
                    HtmlBody = htmlBody,
                };

                var result = await _resend.EmailSendAsync(message);

                if (!result.Success)
                {
                    Log.Error(
                        "Error al enviar correo de cambio de estado de publicación a {Email}",
                        recipientEmail
                    );
                    return false;
                }

                Log.Information(
                    "Correo de cambio de estado de publicación enviado exitosamente a {Email}",
                    recipientEmail
                );
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Error enviando correo de cambio de estado de publicación a {Email}",
                    recipientEmail
                );
                return false;
            }
        }

        private async Task<string> LoadPublicationStatusTemplateAsync(
            int? publicationId,
            string publicationTitle,
            string newStatus
        )
        {
            try
            {
                var templatePath = Path.Combine(
                    _environment.ContentRootPath,
                    "src",
                    "Application",
                    "Templates",
                    "Emails",
                    "PublicationStatusChanged.html"
                );

                var html = await File.ReadAllTextAsync(templatePath);

                html = html.Replace("{{PUBLICATION_TITLE}}", publicationTitle);
                html = html.Replace("{{NEW_STATUS}}", newStatus);

                bool isRejected = newStatus.Equals("Rechazada", StringComparison.OrdinalIgnoreCase);

                if (isRejected && publicationId.HasValue)
                {
                    string appealLink =
                        $"https://bolsafeucn.cl/publications/{publicationId}/appeal";

                    var rejectedBlock =
                        $@"
                <hr />
                <p style='color:#c0392b;'>
                    Tu publicación fue rechazada tras la revisión.
                </p>
                <p>
                    Si consideras que esto fue un error, puedes apelar en el siguiente enlace:
                </p>
                <p>
                    <a href='{appealLink}' style='color:#2980b9;'>
                        Apelar decisión
                    </a>
                </p>
            ";

                    html = html.Replace("{{REJECTED_BLOCK}}", rejectedBlock);
                }
                else
                {
                    html = html.Replace("{{REJECTED_BLOCK}}", string.Empty);
                }

                return html;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error cargando template PublicationStatusChanged");
                throw;
            }
        }

        // --------------------------------------------------------------------
        // 6. *** NUEVO *** EMAIL PARA REVIEW ≤ 3 (EVA-006)
        // --------------------------------------------------------------------
        /// <summary>
        /// Sends an administrative alert email for reviews with low ratings (<= 3).
        /// </summary>
        /// <param name="review">Review DTO describing the low-rated review.</param>
        public async Task<bool> SendLowRatingReviewAlertAsync(ReviewDTO review)
        {
            try
            {
                Log.Information("Enviando alerta de review baja al admin");

                string adminEmail = _configuration["AdminNotifications:Email"]!;
                string fromEmail = _configuration["EmailConfiguration:From"]!;

                // Determinar qué tipo de reseña es y su comentario
                int? rating = review.RatingForStudent ?? review.RatingForOfferor;
                string? comment = review.CommentForStudent ?? review.CommentForOfferor;

                string htmlBody =
                    $@"
                    <h2>Alerta: Nueva reseña crítica</h2>

                    <p><strong>Puntaje:</strong> {rating}</p>
                    <p><strong>Comentario:</strong> {comment}</p>

                    <p><strong>Id Estudiante:</strong> {review.IdStudent}</p>
                    <p><strong>Id Oferente:</strong> {review.IdOfferor}</p>
                    <p><strong>Id Publicación:</strong> {review.IdPublication}</p>

                    <p><strong>¿Llegó a tiempo?:</strong> {(review.AtTime ? "Sí" : "No")}</p>
                    <p><strong>Buena presentación?:</strong> {(review.GoodPresentation ? "Sí" : "No")}</p>

                    <p><strong>Ventana de revisión cierra:</strong> {review.ReviewWindowEndDate}</p>
                ";

                var message = new EmailMessage
                {
                    To = adminEmail,
                    From = fromEmail,
                    Subject = "[ALERTA] Nueva reseña crítica (≤ 3 estrellas)",
                    HtmlBody = htmlBody,
                };

                var result = await _resend.EmailSendAsync(message);

                if (!result.Success)
                {
                    Log.Error("Error enviando alerta de review baja.");
                    return false;
                }

                Log.Information("Alerta de review baja enviada exitosamente al admin.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error enviando alerta de review baja");
                return false;
            }
        }
    }
}
