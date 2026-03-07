using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.AuthDTOs
{
    public class VerifyEmailDTO
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public required string Email { get; set; }

        /// <summary>
        /// Código de verificación enviado al correo del usuario.
        /// </summary>
        [Required(ErrorMessage = "El código de verificación es obligatorio")]
        [RegularExpression(
            @"^\d{6}$",
            ErrorMessage = "El código de verificación debe tener 6 dígitos."
        )]
        public required string VerificationCode { get; set; }
    }
}
