using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.AuthDTOs
{
    public class ResendVerificationDTO
    {
        /// <summary>
        /// Correo electrónico del usuario que solicita reenviar el correo de verificación.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public required string Email { get; set; }
    }
}
