using System.ComponentModel.DataAnnotations;
using backend.src.Application.Validators;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.DTOs.UserDTOs.UserProfileDTOs
{
    public class UpdateCompanyParamsDTO : IUpdateParamsDTO
    {
        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Primer nombre del usuario.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Segundo nombre del usuario.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// RUT del usuario.
        /// </summary>
        [RegularExpression(
            @"^\d{7,8}-[0-9kK]$",
            ErrorMessage = "El Rut debe tener formato XXXXXXXX-X"
        )]
        [RutValidation(ErrorMessage = "El RUT no es válido.")]
        public string? Rut { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string? Email { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Información sobre el usuario.
        /// </summary>
        [MaxLength(
            500,
            ErrorMessage = "La información sobre el usuario debe tener como máximo 500 caracteres"
        )]
        public string? AboutMe { get; set; }

        public void ApplyTo(User user)
        {
            this.Adapt(user);
        }
    }
}
