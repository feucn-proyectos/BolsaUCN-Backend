using System.ComponentModel.DataAnnotations;
using backend.src.Application.Validators;

namespace backend.src.Application.DTOs.AuthDTOs
{
    /// <summary>
    /// DTO para el registro de un usuario individual.
    /// </summary>
    public class RegisterIndividualDTO
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Nombre solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El nombre debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El nombre debe tener máximo 50 letras.")]
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Apellido solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El apellido debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El apellido debe tener máximo 50 letras.")]
        public required string LastName { get; set; }

        /// <summary>
        /// RUT del usuario.
        /// </summary>
        [Required(ErrorMessage = "El RUT es obligatorio.")]
        [RegularExpression(
            @"^\d{7,8}-[0-9kK]$",
            ErrorMessage = "El Rut debe tener formato XXXXXXXX-X"
        )]
        [RutValidation(ErrorMessage = "El RUT no es válido.")]
        public required string Rut { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public required string Email { get; set; }

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

        /// <summary>
        /// Teléfono del usuario.
        /// </summary>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public required string PhoneNumber { get; set; }
    }
}
