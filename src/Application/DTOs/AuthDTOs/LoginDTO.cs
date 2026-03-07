using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.AuthDTOs
{
    public class LoginDTO
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public required string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public required string Password { get; set; }

        /// <summary>
        /// Indica si el usuario desea que la sesión se recuerde.
        /// </summary>
        [Required(ErrorMessage = "El campo RememberMe es obligatorio")]
        public required bool RememberMe { get; set; }
    }
}
