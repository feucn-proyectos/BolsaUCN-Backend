using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/publications")]
    public class OfferController : BaseController
    {
        private readonly IPublicationService _publicationService;
        private readonly IOfferApplicationService _jobApplicationService;

        public OfferController(
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

        /// <summary>
        /// Acepta una postulación a una oferta laboral
        /// </summary>
        /// <param name="applicationId">ID de la postulación a aceptar</param>
        /// <returns>Resultado de la aceptación de la postulación</returns>
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

        /// <summary>
        /// Rechaza una postulación a una oferta laboral
        /// </summary>
        /// <param name="applicationId">ID de la postulación a rechazar</param>
        /// <returns>Resultado del rechazo de la postulación</returns>
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
    }
}
