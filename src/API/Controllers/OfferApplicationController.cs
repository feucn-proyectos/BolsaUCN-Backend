using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/publications")]
    public class OfferApplicationController : BaseController
    {
        private readonly IOfferApplicationService _applicationService;

        public OfferApplicationController(IOfferApplicationService service)
        {
            _applicationService = service;
        }

        //! COMPLETE
        /// <summary>
        /// Permite a un estudiante postularse a una oferta laboral.
        /// </summary>
        /// <param name="offerId">ID de la oferta laboral a la que el estudiante desea postularse.</param>
        /// <returns>Respuesta que entrega un DTO con la aplicación creada.</returns>
        [HttpPost("offers/{offerId}/apply")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> ApplyToOffer(
            int offerId,
            [FromBody] CoverLetterDTO coverLetter
        )
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _applicationService.CreateApplicationAsync(
                parsedUserId,
                offerId,
                coverLetter
            );
            return Ok(new GenericResponse<string>("Aplicación creada exitosamente.", result));
        }

        //! COMPLETE
        /// <summary>
        /// Permite a un estudiante obtener sus postulaciones a ofertas laborales.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar y paginar las postulaciones.</param>
        /// <returns>Respuesta que entrega un DTO con las postulaciones del estudiante.</returns>
        [HttpGet("my-applications")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> GetMyApplications([FromQuery] SearchParamsDTO searchParams)
        {
            int parsedUserId = GetUserIdFromToken();
            var applications = await _applicationService.GetApplicationsByUserIdAsync(
                parsedUserId,
                searchParams
            );
            return Ok(
                new GenericResponse<ApplicationsForApplicantDTO>(
                    "Aplicaciones obtenidas exitosamente.",
                    applications
                )
            );
        }

        /// <summary>
        /// Permite a un estudiante obtener los detalles de una postulación específica.
        /// </summary>
        /// <param name="applicationId">ID de la postulación.</param>
        /// <returns>Respuesta que entrega un DTO con los detalles de la postulación.</returns>
        [HttpGet("my-applications/{applicationId}")]
        [Authorize(Roles = RoleNames.Applicant)]
        public async Task<IActionResult> GetApplicationDetails(int applicationId)
        {
            var parsedUserId = GetUserIdFromToken();
            var detailsResult = await _applicationService.GetApplicationDetailsForApplicantAsync(
                parsedUserId,
                applicationId
            );
            return Ok(
                new GenericResponse<GetApplicationDetailsDTO>(
                    "Detalles de la postulación obtenidos exitosamente.",
                    detailsResult
                )
            );
        }

        /// <summary>
        /// Permite a un estudiante actualizar los detalles de una postulación específica.
        /// </summary>
        /// <param name="applicationId">ID de la postulación a actualizar.</param>
        /// <param name="updateDto">DTO con los detalles actualizados de la postulación.</param>
        /// <returns>Respuesta que indica el resultado de la operación de actualización.</returns>
        [HttpPatch("my-applications/{applicationId}")]
        [Authorize(Roles = RoleNames.Applicant)]
        public async Task<IActionResult> UpdateApplicationDetails(
            int applicationId,
            UpdateApplicationDetailsDTO updateDto
        )
        {
            var parsedUserId = GetUserIdFromToken();
            var updateResult = await _applicationService.UpdateApplicationDetailsAsync(
                parsedUserId,
                applicationId,
                updateDto
            );
            return Ok(
                new GenericResponse<string>(
                    "Detalles de la postulación actualizados exitosamente.",
                    updateResult
                )
            );
        }
    }
}
