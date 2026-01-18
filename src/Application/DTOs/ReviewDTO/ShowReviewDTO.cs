namespace backend.src.Application.DTOs.ReviewDTO
{
    public class ShowReviewDTO
    {
        public required int IdReview { get; set; }
        public required string StudentName { get; set; }
        public required string OfferorName { get; set; }
        public required int RatingForStudent { get; set; }
        public required string CommentForStudent { get; set; }
        public required int RatingForOfferor { get; set; }
        public required string CommentForOfferor { get; set; }
        public required bool AtTime { get; set; }
        public required bool GoodPresentation { get; set; }
        public required bool StudentHasRespectOfferor { get; set; }
        public required bool IsCompleted { get; set; }
        public required bool IsReviewForStudentCompleted { get; set; }
        public required bool IsReviewForOfferorCompleted { get; set; }
        public required bool HasReviewForOfferorBeenDeleted { get; set; }
        public required bool HasReviewForStudentBeenDeleted { get; set; }
        public bool IsClosed { get; set; }
    }
}
