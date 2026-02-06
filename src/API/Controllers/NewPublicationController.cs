using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs.ApplicationsForOfferorDTOs;
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
    }
}
