using System.ComponentModel.DataAnnotations;
using backend.src.Application.Validators;

namespace backend.src.Application.DTOs.AuthDTOs
{
    public class RegisterCompanyDTO
    {
        /// <summary>
        /// Nombre de la compañia.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Nombre solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El nombre debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El nombre debe tener máximo 50 letras.")]
        public required string CompanyName { get; set; }

        /// <summary>
        /// Razon social de la compañia.
        /// </summary>
        [Required(ErrorMessage = "La razon social es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "la Razon social solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "La razon social debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "La razon social debe tener máximo 50 letras.")]
        public required string LegalName { get; set; }

        /// <summary>
        /// RUT de la compañia.
        /// </summary>
        [Required(ErrorMessage = "El RUT es obligatorio.")]
        [RegularExpression(
            @"^\d{8}-[0-9kK]$",
            ErrorMessage = "El Rut debe tener formato XXXXXXXX-X"
        )]
        [RutValidation(ErrorMessage = "El RUT no es válido.")]
        public required string Rut { get; set; }

        /// <summary>
        /// Teléfono de la compañia.
        /// </summary>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// Correo electrónico de la compañia.
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
    }
}
