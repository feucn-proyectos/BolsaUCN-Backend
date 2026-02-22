namespace backend.src.Application.DTOs.ReviewDTO.AdminDTOs
{
    public class GetReviewDTO
    {
        public int ReviewId { get; set; }
        public string JobOfferTitle { get; set; } = null!;
        public string ReviewStatus { get; set; } = null!;
        public DateTime OpenUntil { get; set; }
        public bool HasReviewBeenActionedByAdmin { get; set; }
    }
}
