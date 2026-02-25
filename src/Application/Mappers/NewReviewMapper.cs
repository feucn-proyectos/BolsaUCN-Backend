using backend.src.Application.DTOs.ReviewDTO.AdminDTOs;
using backend.src.Application.DTOs.ReviewDTO.CreateReviewDTOs;
using backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO;
using backend.src.Application.DTOs.ReviewDTO.ReviewReportDTO;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.Mappers
{
    public class NewReviewMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureCreateReviewMappings();
            ConfigureGetReviewsMappings();
            ConfigureReportMappings();
        }

        public void ConfigureCreateReviewMappings()
        {
            TypeAdapterConfig<ApplicantReviewForOfferorDTO, NewReview>
                .NewConfig()
                .Map(dest => dest.ApplicantRatingOfOfferor, src => src.Rating)
                .Map(dest => dest.ApplicantCommentForOfferor, src => src.Comment)
                .Map(dest => dest.ApplicantReviewCompletedAt, src => DateTime.UtcNow);

            TypeAdapterConfig<OfferorReviewForApplicantDTO, NewReview>
                .NewConfig()
                .Map(dest => dest.OfferorRatingOfApplicant, src => src.Rating)
                .Map(dest => dest.OfferorCommentForApplicant, src => src.Comment)
                .Map(dest => dest.IsOnTime, src => src.IsOnTime)
                .Map(dest => dest.IsPresentable, src => src.IsPresentable)
                .Map(dest => dest.IsRespectful, src => src.IsRespectful)
                .Map(dest => dest.OfferorReviewCompletedAt, src => DateTime.UtcNow);
        }

        public void ConfigureGetReviewsMappings()
        {
            TypeAdapterConfig<NewReview, MyReviewDTO>
                .NewConfig()
                .Map(dest => dest.ReviewId, src => src.Id)
                .Map(dest => dest.ReviewStatus, src => src.CurrentStatus.ToString())
                .Map(dest => dest.JobOfferTitle, src => src.Application!.JobOffer!.Title)
                .Map(dest => dest.OpenUntil, src => src.Application!.JobOffer!.ReviewDeadline)
                .Map(dest => dest.HasReviewBeenActionedByAdmin, src => src.IsReviewActionedByAdmin)
                .Map(dest => dest.ApplicantId, src => src.ApplicantId)
                .Map(dest => dest.ApplicantFullName, src => src.Applicant!.FullName)
                .Map(dest => dest.OfferorId, src => src.OfferorId)
                .Map(dest => dest.OfferorFullName, src => src.Offeror!.FullName);

            TypeAdapterConfig<NewReview, MyReviewDetailsDTO>
                .NewConfig()
                .Map(dest => dest.ReviewId, src => src.Id)
                .Map(dest => dest.OfferorCommentForApplicant, src => src.OfferorCommentForApplicant)
                .Map(dest => dest.OfferorRatingOfApplicant, src => src.OfferorRatingOfApplicant)
                .Map(dest => dest.ApplicantCommentForOfferor, src => src.ApplicantCommentForOfferor)
                .Map(dest => dest.ApplicantRatingOfOfferor, src => src.ApplicantRatingOfOfferor)
                .Map(dest => dest.IsOnTime, src => src.IsOnTime)
                .Map(dest => dest.IsPresentable, src => src.IsPresentable)
                .Map(dest => dest.IsRespectful, src => src.IsRespectful)
                .Map(dest => dest.OfferorReviewCompletedAt, src => src.OfferorReviewCompletedAt)
                .Map(dest => dest.ApplicantReviewCompletedAt, src => src.ApplicantReviewCompletedAt)
                .Map(dest => dest.ReviewClosedAt, src => src.ReviewClosedAt)
                .Map(
                    dest => dest.IsOfferorReviewForApplicantHidden,
                    src => src.IsOfferorReviewForApplicantHidden
                )
                .Map(dest => dest.OfferorReviewHiddenAt, src => src.OfferorReviewHiddenAt)
                .Map(
                    dest => dest.IsApplicantReviewForOfferorHidden,
                    src => src.IsApplicantReviewForOfferorHidden
                )
                .Map(dest => dest.ApplicantReviewHiddenAt, src => src.ApplicantReviewHiddenAt)
                .Map(dest => dest.JobOfferId, src => src.Application!.JobOfferId)
                .Map(dest => dest.JobOfferTitle, src => src.Application!.JobOffer!.Title)
                .Map(dest => dest.ApplicationId, src => src.ApplicationId)
                .Map(dest => dest.ApplicantId, src => src.ApplicantId)
                .Map(
                    dest => dest.ApplicantFullName,
                    src => src.Applicant!.FirstName + " " + src.Applicant.LastName
                )
                .Map(dest => dest.OfferorId, src => src.OfferorId)
                .Map(
                    dest => dest.OfferorFullName,
                    src => src.Offeror!.FirstName + " " + src.Offeror.LastName
                )
                .Map(dest => dest.OpenUntil, src => src.Application!.JobOffer!.ReviewDeadline)
                .Map(dest => dest.ReviewStatus, src => src.CurrentStatus.ToString());

            TypeAdapterConfig<NewReview, GetReviewDTO>
                .NewConfig()
                .Map(dest => dest.ReviewId, src => src.Id)
                .Map(dest => dest.ReviewStatus, src => src.CurrentStatus.ToString())
                .Map(dest => dest.JobOfferTitle, src => src.Application!.JobOffer!.Title)
                .Map(dest => dest.OpenUntil, src => src.Application!.JobOffer!.ReviewDeadline)
                .Map(dest => dest.ApplicantFullName, src => src.Applicant!.FullName)
                .Map(dest => dest.OfferorFullName, src => src.Offeror!.FullName)
                .Map(dest => dest.HasReviewBeenActionedByAdmin, src => src.IsReviewActionedByAdmin);

            TypeAdapterConfig<NewReview, GetReviewDetailsDTO>
                .NewConfig()
                .Map(dest => dest.ReviewId, src => src.Id)
                .Map(dest => dest.OfferorCommentForApplicant, src => src.OfferorCommentForApplicant)
                .Map(dest => dest.OfferorRatingOfApplicant, src => src.OfferorRatingOfApplicant)
                .Map(dest => dest.ApplicantCommentForOfferor, src => src.ApplicantCommentForOfferor)
                .Map(dest => dest.ApplicantRatingOfOfferor, src => src.ApplicantRatingOfOfferor)
                .Map(dest => dest.IsOnTime, src => src.IsOnTime)
                .Map(dest => dest.IsPresentable, src => src.IsPresentable)
                .Map(dest => dest.IsRespectful, src => src.IsRespectful)
                .Map(dest => dest.OfferorReviewCompletedAt, src => src.OfferorReviewCompletedAt)
                .Map(dest => dest.ApplicantReviewCompletedAt, src => src.ApplicantReviewCompletedAt)
                .Map(dest => dest.ReviewClosedAt, src => src.ReviewClosedAt)
                .Map(
                    dest => dest.IsOfferorReviewForApplicantHidden,
                    src => src.IsOfferorReviewForApplicantHidden
                )
                .Map(dest => dest.OfferorReviewHiddenAt, src => src.OfferorReviewHiddenAt)
                .Map(
                    dest => dest.IsApplicantReviewForOfferorHidden,
                    src => src.IsApplicantReviewForOfferorHidden
                )
                .Map(dest => dest.ApplicantReviewHiddenAt, src => src.ApplicantReviewHiddenAt)
                .Map(dest => dest.JobOfferId, src => src.Application!.JobOfferId)
                .Map(dest => dest.JobOfferTitle, src => src.Application!.JobOffer!.Title)
                .Map(dest => dest.ApplicationId, src => src.ApplicationId)
                .Map(dest => dest.ApplicantId, src => src.ApplicantId)
                .Map(
                    dest => dest.ApplicantFullName,
                    src => src.Applicant!.FirstName + " " + src.Applicant.LastName
                )
                .Map(dest => dest.OfferorId, src => src.OfferorId)
                .Map(
                    dest => dest.OfferorFullName,
                    src => src.Offeror!.FirstName + " " + src.Offeror.LastName
                )
                .Map(dest => dest.OpenUntil, src => src.Application!.JobOffer!.ReviewDeadline)
                .Map(dest => dest.ReviewStatus, src => src.CurrentStatus.ToString());
        }

        public void ConfigureReportMappings()
        {
            TypeAdapterConfig<NewReview, SystemReviewDetailDTO>
                .NewConfig()
                .Map(dest => dest.ReviewId, src => src.Id)
                .Map(dest => dest.PublicationTitle, src => src.Application!.JobOffer!.Title)
                .Map(dest => dest.ApplicantName, src => src.Applicant!.FullName)
                .Map(dest => dest.OfferorName, src => src.Offeror!.FullName)
                .Map(dest => dest.RatingForStudent, src => src.OfferorRatingOfApplicant)
                .Map(dest => dest.CommentForStudent, src => src.OfferorCommentForApplicant)
                .Map(dest => dest.RatingForOfferor, src => src.ApplicantRatingOfOfferor)
                .Map(dest => dest.CommentForOfferor, src => src.ApplicantCommentForOfferor)
                .Map(dest => dest.ReviewDate, src => src.CreatedAt)
                .Map(dest => dest.IsOnTime, src => src.IsOnTime)
                .Map(dest => dest.IsPresentable, src => src.IsPresentable)
                .Map(dest => dest.IsRespectful, src => src.IsRespectful)
                .Map(dest => dest.IsCompleted, src => src.CurrentStatus == ReviewStatus.Completada)
                .Map(dest => dest.IsClosed, src => src.CurrentStatus == ReviewStatus.Cerrada);
        }
    }
}
