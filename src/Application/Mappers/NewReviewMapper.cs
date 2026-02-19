using backend.src.Application.DTOs.ReviewDTO.CreateReviewDTOs;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.Mappers
{
    public class NewReviewMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureCreateReviewMappings();
        }

        public void ConfigureCreateReviewMappings()
        {
            TypeAdapterConfig<ApplicantReviewForOfferorDTO, NewReview>
                .NewConfig()
                .Map(dest => dest.ApplicantRatingOfOfferor, src => src.Rating)
                .Map(dest => dest.ApplicantCommentForOfferor, src => src.Comment)
                .Map(dest => dest.ApplicantReviewCompletedAt, src => DateTime.UtcNow);
        }
    }
}
