using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.UserDTOs
{
    public class ChangeUserEmailDTO
    {
        /// <summary>
        /// Nueva dirección de correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El nuevo correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El nuevo correo electrónico no es válido.")]
        public required string NewEmail { get; set; }

        /// <summary>
        /// Contraseña actual del usuario para verificar la identidad.
        /// </summary>
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        public required string CurrentPassword { get; set; }
    }
}
