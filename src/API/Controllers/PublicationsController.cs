using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs.ApplicationsForOfferorDTOs;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/publications")]
    public class NewPublicationController : BaseController
    {
        private readonly IPublicationService _publicationService;
        private readonly IOfferApplicationService _applicationService;

        public NewPublicationController(
            IPublicationService publicationService,
            IOfferApplicationService applicationService
        )
        {
            _publicationService = publicationService;
            _applicationService = applicationService;
        }

        #region Crear Publicación
        /// <summary>
        /// Crea una nueva oferta laboral
        /// </summary>
        /// <param name="offerDto">Datos de la oferta a crear</param>
        /// <returns>Resultado de la creación de la oferta con el ID generado</returns>
        [HttpPost("offers")]
        [Authorize]
        public async Task<IActionResult> CreateOffer([FromBody] CreateOfferDTO offerDto)
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _publicationService.CreateOfferAsync(offerDto, parsedUserId);
            return Ok(new GenericResponse<string>("Oferta creada exitosamente.", result));
        }
        #endregion

        #region Punto de vista del oferente
        /// <summary>
        /// Obtiene las publicaciones del usuario autenticado
        /// </summary>
        /// <returns>Lista de publicaciones del usuario</returns>
        [HttpGet("my-publications")]
        [Authorize(Roles = RoleNames.Offeror)]
        public async Task<IActionResult> GetPublicationsForOfferer(
            [FromQuery] MyPublicationsSeachParamsDTO searchParamsDTO
        )
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _publicationService.GetMyPublicationsAsync(
                parsedUserId,
                searchParamsDTO
            );
            return Ok(new GenericResponse<MyPublicationsDTO>("Publicaciones obtenidas", result));
        }

        [HttpGet("my-publications/{publicationId}")]
        [Authorize(Roles = RoleNames.Offeror)]
        public async Task<IActionResult> GetPublicationDetailsForOfferer(int publicationId)
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _publicationService.GetPublicationDetailsForOffererAsync(
                publicationId,
                parsedUserId
            );
            return Ok(
                new GenericResponse<MyPublicationDetailsDTO>(
                    "Detalles de la publicación obtenidos exitosamente.",
                    result
                )
            );
        }

        [HttpGet("explore/offers")]
        public async Task<IActionResult> ExploreOffers(
            [FromQuery] ExploreOffersSearchParamsDTO searchParams
        )
        {
            int? parsedUserId = GetUserIdFromTokenNullable();
            var result = await _publicationService.GetOffersAsync(searchParams, parsedUserId);
            return Ok(
                new GenericResponse<OffersForApplicantDTO>(
                    "Ofertas obtenidas exitosamente.",
                    result
                )
            );
        }

        [HttpGet("explore/offers/{publicationId}/public")]
        public async Task<IActionResult> GetOfferDetailsPublic(int publicationId)
        {
            var result = await _publicationService.GetOfferDetailsForPublicAsync(publicationId);
            return Ok(
                new GenericResponse<OfferDetailsForPublicDTO>(
                    "Detalles de la oferta obtenidos exitosamente.",
                    result
                )
            );
        }

        [HttpGet("explore/offers/{publicationId}")]
        [Authorize]
        public async Task<IActionResult> GetOfferDeatailsForApplicant(int publicationId)
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _publicationService.GetOfferDetailsForApplicantAsync(
                publicationId,
                parsedUserId
            );
            return Ok(
                new GenericResponse<OfferDetailsForApplicantDTO>(
                    "Detalles de la oferta obtenidos exitosamente.",
                    result
                )
            );
        }

        [HttpGet("explore/buysells")]
        public async Task<IActionResult> ExploreBuySells(
        //TODO [FromQuery] ExploreBuySellSearchParamsDTO searchParams
        )
        {
            return Ok(
                new GenericResponse<string>(
                    "Funcionalidad de exploración de Compra/Venta aún no implementada.",
                    null
                )
            );
        }

        [HttpGet("explore/buysells/{publicationId}/public")]
        public async Task<IActionResult> GetBuySellDetailsPublic(int publicationId)
        {
            return Ok(
                new GenericResponse<string>(
                    "Funcionalidad de detalles de Compra/Venta aún no implementada.",
                    null
                )
            );
        }

        [HttpGet("explore/buysells/{publicationId}")]
        [Authorize(Roles = RoleNames.Applicant)]
        public async Task<IActionResult> GetBuySellDetailsForApplicant(int publicationId)
        {
            return Ok(
                new GenericResponse<string>(
                    "Funcionalidad de detalles de Compra/Venta para usuario aún no implementada.",
                    null
                )
            );
        }

        [HttpGet("my-publications/{offerId}/applications")]
        [Authorize(Roles = RoleNames.Offeror)]
        public async Task<IActionResult> GetAllApplicationsByOfferId(
            int offerId,
            [FromQuery] ApplicationsForOfferorSearchParamsDTO searchParams
        )
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _applicationService.GetAllApplicationsByOfferIdAsync(
                offerId,
                parsedUserId,
                searchParams
            );
            return Ok(
                new GenericResponse<ApplicationsForOfferorDTO>(
                    "Aplicaciones obtenidas exitosamente.",
                    result
                )
            );
        }

        [HttpGet("my-publications/{offerId}/applications/{applicationId}/cv")]
        public async Task<IActionResult> DownloadApplicantCV(int offerId, int applicationId)
        {
            int parsedUserId = GetUserIdFromToken();
            var (fileStream, contentType, fileName) = await _applicationService.GetApplicantCVAsync(
                offerId,
                applicationId,
                parsedUserId
            );
            return File(fileStream, contentType, fileName);
        }

        [HttpPatch("my-publications/{offerId}/applications/{applicationId}/update-status")]
        [Authorize(Roles = RoleNames.Offeror)]
        public async Task<IActionResult> UpdateApplicationStatus(
            int offerId,
            int applicationId,
            [FromBody] UpdateApplicationStatusDTO statusDto
        )
        {
            int parsedOffererId = GetUserIdFromToken();
            var result = await _applicationService.UpdateApplicationStatusByOfferorAsync(
                parsedOffererId,
                applicationId,
                offerId,
                statusDto
            );
            return Ok(
                new GenericResponse<string>(
                    "Estado de la aplicación actualizado exitosamente.",
                    result
                )
            );
        }

        [HttpPatch("my-publications/{publicationId}/close")]
        [Authorize(Roles = RoleNames.Offeror)]
        public async Task<IActionResult> ClosePublicationManually(int publicationId)
        {
            int parsedOffererId = GetUserIdFromToken();
            var result = await _publicationService.ClosePublicationManuallyAsync(
                publicationId,
                parsedOffererId
            );
            return Ok(new GenericResponse<string>("Publicación cerrada exitosamente.", result));
        }

        [HttpPost("my-publications/{publicationId}/appeal")]
        [Authorize(Roles = RoleNames.Offeror)]
        public async Task<IActionResult> AppealPublication(
            int publicationId,
            [FromForm] AppealRejectionDTO appealDTO
        )
        {
            int parsedOfferorId = GetUserIdFromToken();
            var result = await _publicationService.AppealRejectedPublicationAsync(
                publicationId,
                parsedOfferorId,
                appealDTO
            );
            return Ok(new GenericResponse<string>("Publicación apelada exitosamente.", result));
        }
        #endregion
    }
}
