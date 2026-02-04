using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
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
        private readonly IOfferApplicationService _jobApplicationService;

        public NewPublicationController(
            IPublicationService publicationService,
            IOfferApplicationService jobApplicationService
        )
        {
            _publicationService = publicationService;
            _jobApplicationService = jobApplicationService;
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

        [HttpPatch("offerent/applications/{applicationId}/accept")]
        [Authorize]
        public async Task<IActionResult> AcceptApplication(int applicationId)
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _jobApplicationService.UpdateApplicationStatusAsync(
                applicationId,
                ApplicationStatus.Aceptada,
                parsedUserId
            );
            return Ok(new GenericResponse<bool>("Aplicación aceptada exitosamente.", result));
        }

        [HttpPatch("offerent/applications/{applicationId}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectApplication(int applicationId)
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _jobApplicationService.UpdateApplicationStatusAsync(
                applicationId,
                ApplicationStatus.Rechazada,
                parsedUserId
            );
            return Ok(new GenericResponse<bool>("Aplicación rechazada exitosamente.", result));
        }

        /// <summary>
        /// Obtiene las publicaciones del usuario autenticado
        /// </summary>
        /// <returns>Lista de publicaciones del usuario</returns>
        [HttpGet("my-publications")]
        [Authorize(Roles = RoleNames.Offeror)]
        public async Task<IActionResult> GetPublicationsForOfferer(
            [FromForm] MyPublicationsSeachParamsDTO searchParamsDTO
        )
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _publicationService.GetMyPublicationsAsync(parsedUserId, searchParamsDTO);
            return Ok(new GenericResponse<MyPublicationsDTO>("Publicaciones obtenidas", result));
        }
    }
}
