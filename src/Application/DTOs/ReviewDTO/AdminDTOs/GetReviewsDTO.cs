namespace backend.src.Application.DTOs.ReviewDTO.AdminDTOs
{
    public class GetReviewsDTO
    {
        public required List<GetReviewDTO> Reviews { get; set; }
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
        public required int CurrentPage { get; set; }
        public required int PageSize { get; set; }
    }
}
