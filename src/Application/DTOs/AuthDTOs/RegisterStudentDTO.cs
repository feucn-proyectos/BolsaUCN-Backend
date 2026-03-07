using System.ComponentModel.DataAnnotations;
using backend.src.Application.Validators;

namespace backend.src.Application.DTOs.AuthDTOs
{
    public class RegisterStudentDTO
    {
        /// <summary>
        /// Nombre completo del estudiante.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido completo del estudiante.
        /// </summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public required string LastName { get; set; }

        /// <summary>
        /// Correo institucional del estudiante.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public required string Email { get; set; }

        /// <summary>
        /// RUT del estudiante.
        /// </summary>
        [Required(ErrorMessage = "El RUT es obligatorio.")]
        [RegularExpression(
            @"^\d{7,8}-[0-9kK]$",
            ErrorMessage = "El Rut debe tener formato XXXXXXXX-X"
        )]
        [RutValidation(ErrorMessage = "El RUT no es válido.")]
        public required string Rut { get; set; }

        /// <summary>
        /// Teléfono del estudiante.
        /// </summary>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// Contraseña del estudiante.
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
        /// Confirmación de la contraseña del estudiante.
        /// </summary>
        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmPassword { get; set; }

        /// <summary>
        /// Discapacidad del estudiante.
        /// </summary>
        [Required(ErrorMessage = "La discapacidad es obligatoria.")]
        [RegularExpression(
            @"^(Ninguna|Visual|Auditiva|Motriz|Cognitiva|Otra)$",
            ErrorMessage = "El tipo de discapacidad no es válido."
        )]
        public required string Disability { get; set; }
    }
}
