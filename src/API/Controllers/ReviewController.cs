using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.ReviewDTO.CreateReviewDTOs;
using backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/")]
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;
        private readonly IPdfGeneratorService _pdfGeneratorService;

        public ReviewController(
            IReviewService reviewService,
            IPdfGeneratorService pdfGeneratorService
        )
        {
            _reviewService = reviewService;
            _pdfGeneratorService = pdfGeneratorService;
        }

        #region Creacion de reviews por usuarios

        [HttpPatch("reviews/{reviewId}/applicant")]
        [Authorize(Roles = RoleNames.Applicant)]
        public async Task<IActionResult> CreateApplicantReviewForOfferor(
            int reviewId,
            [FromBody] ApplicantReviewForOfferorDTO reviewDTO
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
            [FromBody] OfferorReviewForApplicantDTO reviewDTO
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

        [HttpGet("reviews")]
        [Authorize(Roles = RoleNames.Applicant + "," + RoleNames.Offeror)]
        public async Task<IActionResult> GetMyReviewsAsync(
            [FromQuery] MyReviewsSearchParamsDTO searchParams
        )
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _reviewService.GetMyReviewsAsync(searchParams, parsedUserId);
            return Ok(new GenericResponse<MyReviewsDTO>("Reviews obtenidas exitosamente", result));
        }

        [HttpGet("reviews/{reviewId}")]
        [Authorize(Roles = RoleNames.Applicant + "," + RoleNames.Offeror)]
        public async Task<IActionResult> GetMyReviewDetailsAsync(int reviewId)
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _reviewService.GetMyReviewDetailsByIdAsync(reviewId, parsedUserId);
            return Ok(
                new GenericResponse<MyReviewDetailsDTO>(
                    "Detalles de review obtenidos exitosamente",
                    result
                )
            );
        }

        [HttpGet("reviews/pdf")]
        [Authorize(Roles = RoleNames.Applicant + "," + RoleNames.Offeror)]
        public async Task<IActionResult> GetMyReviewsPdf()
        {
            int parsedUserId = GetUserIdFromToken();

            var pdfBytes = await _pdfGeneratorService.GenerateUserReviewsPdfAsync(parsedUserId);
            return File(
                pdfBytes,
                "application/pdf",
                $"reviews_{parsedUserId}_{DateTime.Now:yyyyMMdd}.pdf"
            );
        }
        #endregion
    }
}
