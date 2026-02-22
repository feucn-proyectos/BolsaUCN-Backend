namespace backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO
{
    public class MyReviewDTO
    {
        public int ReviewId { get; set; }
        public required string JobOfferTitle { get; set; }
        public required DateTime OpenUntil { get; set; }
        public required string ReviewStatus { get; set; }
        public bool HasReviewBeenActionedByAdmin { get; set; }
    }
}
