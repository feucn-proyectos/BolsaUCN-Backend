using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.ReviewDTO.CreateReviewDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/")]
    public class NewReviewController : BaseController
    {
        private readonly IReviewService _reviewService;

        public NewReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        #region Creacion de reviews por usuarios

        [HttpPatch("reviews/{reviewId}/applicant")]
        [Authorize(Roles = RoleNames.Applicant)]
        public async Task<IActionResult> CreateApplicantReviewForOfferor(
            int reviewId,
            ApplicantReviewForOfferorDTO reviewDTO
        )
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _reviewService.CreateApplicantReviewForOfferorAsync(
                reviewId,
                parsedUserId,
                reviewDTO
            );
            return Ok(new GenericResponse<string>("Review creada exitosamente", result));
        }

        [HttpPatch("reviews/{reviewId}/offeror")]
        [Authorize(Roles = RoleNames.Offeror)]
        public async Task<IActionResult> CreateOfferorReviewForApplicantAsync(
            int reviewId,
            OfferorReviewForApplicantDTO reviewDTO
        )
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _reviewService.CreateOfferorReviewForApplicantAsync(
                reviewId,
                parsedUserId,
                reviewDTO
            );
            return Ok(new GenericResponse<string>("Review creada exitosamente", result));
        }


        #endregion
    }
}
