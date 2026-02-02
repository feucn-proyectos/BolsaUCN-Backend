using System.ComponentModel.DataAnnotations;
using backend.src.Application.Validators;

namespace backend.src.Application.DTOs.UserDTOs.UserProfileDTOs
{
    /// <summary>
    /// DTO para actualizar el perfil de un usuario.
    /// </summary>
    public class UpdateUserProfileDTO
    {
        [MinLength(3, ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres.")]
        [MaxLength(20, ErrorMessage = "El nombre de usuario debe tener como máximo 20 caracteres.")]
        public string? UserName { get; set; }

        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
        [MaxLength(50, ErrorMessage = "El nombre debe tener como máximo 50 caracteres.")]
        public string? FirstName { get; set; }

        [MinLength(2, ErrorMessage = "El apellido debe tener al menos 2 caracteres.")]
        [MaxLength(50, ErrorMessage = "El apellido debe tener como máximo 50 caracteres.")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string? Email { get; set; }

        [MaxLength(
            500,
            ErrorMessage = "La descripción 'Sobre mí' debe tener como máximo 500 caracteres."
        )]
        public string? AboutMe { get; set; }

        [PhoneValidation(ErrorMessage = "El número de teléfono no es válido.")]
        public string? PhoneNumber { get; set; }
    }
}
