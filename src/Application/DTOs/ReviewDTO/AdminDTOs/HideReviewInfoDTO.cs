using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.ReviewDTO.AdminDTOs
{
    public class HideReviewInfoDTO
    {
        public bool? HideOfferorReviewForApplicant { get; set; }

        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s.,;:!?'""()\-]*$",
            ErrorMessage = "La razón contiene caracteres no permitidos."
        )]
        public string? OfferorReviewHiddenReason { get; set; }

        public bool? HideApplicantReviewForOfferor { get; set; }

        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s.,;:!?'""()\-]*$",
            ErrorMessage = "La razón contiene caracteres no permitidos."
        )]
        public string? ApplicantReviewHiddenReason { get; set; }
    }
}
