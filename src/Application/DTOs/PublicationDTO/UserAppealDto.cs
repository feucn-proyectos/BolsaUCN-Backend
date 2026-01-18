using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO para que un usuario apele el rechazo de su publicación
    /// </summary>
    public class UserAppealDto
    {
        /// <summary>
        /// Justificación de la apelación proporcionada por el usuario.
        /// </summary>
        [Required(ErrorMessage = "Debe justificar su apelación.")]
        [StringLength(
            1000,
            MinimumLength = 20,
            ErrorMessage = "La justificación debe tener entre 20 y 1000 caracteres."
        )]
        public required string Justification { get; set; }
    }
}
