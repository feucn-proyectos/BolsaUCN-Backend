using System.Security.Claims;
using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.JobAplicationDTO;
using backend.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace backend.src.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar postulaciones a ofertas laborales
    /// </summary>
    [Route("api/job-applications")]
    [ApiController]
    public class JobApplicationController : ControllerBase
    {
        private readonly IJobApplicationService _jobApplicationService;

        public JobApplicationController(IJobApplicationService jobApplicationService)
        {
            _jobApplicationService = jobApplicationService;
        }

        /* === LEGACY ENDPOINTS. NO ELIMINAR HASTA REFACTORIZAR EL FRONTEND === */

        //? IN USE BY: BolsaFeUCN/frontend/src/app/jobs/history/page.tsx
        [HttpGet("{applicationId}/details")]
        [Authorize]
        public async Task<ActionResult<JobApplicationDetailDto>> GetApplicationDetails(
            int applicationId
        )
        {
            var application = await _jobApplicationService.GetApplicationDetailAsync(applicationId);
            if (application == null)
                return NotFound("Postulación no encontrada");

            return Ok(application);
        }
    }
}
