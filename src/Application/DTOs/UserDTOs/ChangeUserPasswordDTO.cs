using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.UserDTOs
{
    public class ChangeUserPasswordDTO
    {
        /// <summary>
        /// Contraseña actual del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public required string CurrentPassword { get; set; }

        /// <summary>
        /// Nueva contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public required string NewPassword { get; set; }

        /// <summary>
        /// Confirmación de la nueva contraseña del usuario.
        /// </summary>
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmNewPassword { get; set; }
    }
}
