namespace backend.src.Application.DTOs.ReviewDTO.AdminDTOs
{
    public class HideReviewInfoDTO
    {
        public bool? HideOfferorReviewForApplicant { get; set; }
        public string? OfferorReviewHiddenReason { get; set; }
        public bool? HideApplicantReviewForOfferor { get; set; }
        public string? ApplicantReviewHiddenReason { get; set; }
    }
}
