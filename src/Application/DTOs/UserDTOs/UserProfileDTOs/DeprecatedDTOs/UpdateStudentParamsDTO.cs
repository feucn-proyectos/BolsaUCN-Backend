using System.ComponentModel.DataAnnotations;
using backend.src.Application.DTOs.UserDTOs;
using backend.src.Application.Validators;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.DTOs.UserDTOs.UserProfileDTOs
{
    public class UpdateStudentParamsDTO : IUpdateParamsDTO
    {
        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        ///
        public string? UserName { get; set; }

        /// <summary>
        /// Primer nombre del usuario.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Segundo nombre del usuario.
        /// </summary>
        public string? LastName { get; set; }

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

        /// <summary>
        /// Aplica los cambios del DTO al usuario dado.
        /// </summary>
        /// <param name="user">Usuario al que se le aplicarán los cambios.</param>
        public void ApplyTo(User user)
        {
            this.Adapt(user);
        }
    }
}
