using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO para que un administrador rechace una publicación
    /// </summary>
    public class AdminRejectDto
    {
        /// <summary>
        /// Razón del rechazo proporcionada por el administrador.
        /// </summary>
        [Required(ErrorMessage = "Debe proporcionar una razón para el rechazo.")]
        [StringLength(
            500,
            MinimumLength = 10,
            ErrorMessage = "La razón debe tener entre 10 y 500 caracteres."
        )]
        public required string Reason { get; set; }
    }
}
