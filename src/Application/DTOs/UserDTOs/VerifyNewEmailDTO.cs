namespace backend.src.Application.DTOs.UserDTOs
{
    using System.ComponentModel.DataAnnotations;

    public class VerifyNewEmailDTO
    {
        /// <summary>
        /// Código de verificación enviado al nuevo correo electrónico.
        /// </summary>
        [Required(ErrorMessage = "El código de verificación es obligatorio.")]
        [StringLength(
            6,
            ErrorMessage = "El código de verificación debe tener 6 caracteres.",
            MinimumLength = 6
        )]
        [RegularExpression(
            "^[0-9]{6}$",
            ErrorMessage = "El código de verificación debe ser numérico."
        )]
        public required string VerificationCode { get; set; }
    }
}
