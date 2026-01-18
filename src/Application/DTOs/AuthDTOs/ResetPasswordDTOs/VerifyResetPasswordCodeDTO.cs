using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.AuthDTOs.ResetPasswordDTOs
{
    public class VerifyResetPasswordCodeDTO
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public required string Email { get; set; }

        /// <summary>
        /// Código de reseteo de contraseña enviado al correo del usuario.
        /// </summary>
        [Required(ErrorMessage = "El código es obligatorio")]
        [RegularExpression(
            @"^\d{6}$",
            ErrorMessage = "El código de verificación debe tener 6 dígitos."
        )]
        public required string VerificationCode { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?]).*$",
            ErrorMessage = "La contraseña debe ser alfanumérica y contener al menos una mayúscula y al menos un caracter especial."
        )]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [MaxLength(20, ErrorMessage = "La contraseña debe tener como máximo 20 caracteres")]
        public required string Password { get; set; }

        /// <summary>
        /// Confirmación de la contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmPassword { get; set; }
    }
}
