using backend.src.Application.DTOs.PublicationDTO;

namespace backend.src.Application.DTOs.ReviewDTO
{
    public class PublicationAndReviewInfoDTO
    {
        public required PublicationsDTO Publication { get; set; }
        public required ShowReviewDTO Review { get; set; }
    }
}
