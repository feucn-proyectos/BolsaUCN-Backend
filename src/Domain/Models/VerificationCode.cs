namespace backend.src.Domain.Models
{
    /// <summary>
    /// Tipos de códigos de verificación.
    /// </summary>
    public enum CodeType
    {
        EmailConfirmation,
        PasswordReset,
        EmailChange,
    }

    /// <summary>
    /// Clase que representa un código de verificación para acciones como confirmación de correo o restablecimiento de contraseña.
    /// </summary>
    public class VerificationCode : ModelBase
    {
        public required string Code { get; set; }
        public required CodeType CodeType { get; set; }
        public required int UserId { get; set; }
        public int Attempts { get; set; } = 0;
        public required DateTime Expiration { get; set; }
    }
}
