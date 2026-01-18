using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.JobAplicationDTO;
using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/publications")]
    public class ApplicationController : BaseController
    {
        private readonly IJobApplicationService _service;

        public ApplicationController(IJobApplicationService service)
        {
            _service = service;
        }

        //! COMPLETE
        /// <summary>
        /// Permite a un estudiante postularse a una oferta laboral.
        /// </summary>
        /// <param name="offerId">ID de la oferta laboral a la que el estudiante desea postularse.</param>
        /// <returns>Respuesta que entrega un DTO con la aplicación creada.</returns>
        [HttpPost("offers/{offerId}/apply")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> ApplyToOffer(int offerId)
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _service.CreateApplicationAsync(parsedUserId, offerId);
            return Ok(
                new GenericResponse<JobApplicationResponseDto>(
                    "Aplicación creada exitosamente.",
                    result
                )
            );
        }

        //! COMPLETE
        //? NOT IN USE BY FRONTEND. TO REPLACE LEGACY ENDPOINT IN JobApplicationController
        //? LEGACY RESPONSE: IEnumerable<JobApplicationResponseDto>
        /// <summary>
        /// Obtiene todas las aplicaciones realizadas por el estudiante autenticado.
        /// </summary>
        /// <returns>Respuesta que entrega un DTO con las aplicaciones del estudiante.</returns>
        [HttpGet("offers/my-applications")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> GetMyApplications([FromBody] SearchParamsDTO searchParams)
        {
            int parsedUserId = GetUserIdFromToken();
            var applications = await _service.GetUserApplicationsByIdAsync(
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
    }
}
