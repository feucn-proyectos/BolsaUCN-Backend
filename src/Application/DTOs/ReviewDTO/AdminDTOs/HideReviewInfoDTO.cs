namespace backend.src.Application.DTOs.ReviewDTO.AdminDTOs
{
    public class HideReviewInfoDTO
    {
        public bool? HideOfferorRating { get; set; }
        public bool? HideOfferorComment { get; set; }
        public bool? HideApplicantRating { get; set; }
        public bool? HideApplicantComment { get; set; }
    }
}
