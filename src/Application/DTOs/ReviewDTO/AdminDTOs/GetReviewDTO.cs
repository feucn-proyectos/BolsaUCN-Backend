namespace backend.src.Application.DTOs.ReviewDTO.AdminDTOs
{
    public class GetReviewDTO
    {
        public required int ReviewId { get; set; }
        public required string JobOfferTitle { get; set; }
        public required string ReviewStatus { get; set; }
        public required DateTime OpenUntil { get; set; }
        public required string ApplicantFullName { get; set; }
        public required string OfferorFullName { get; set; }
        public required bool HasReviewBeenActionedByAdmin { get; set; }
    }
}
