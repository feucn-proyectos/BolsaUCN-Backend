using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.UserDTOs
{
    public class GetPhotoDTO
    {
        [Required(ErrorMessage = "La URL de la foto es obligatoria")]
        public string PhotoUrl { get; set; } = null!;
    }
}
