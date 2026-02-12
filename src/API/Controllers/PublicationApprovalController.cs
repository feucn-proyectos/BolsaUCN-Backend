using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using PendingPublicationSearchParamsDTO = backend.src.Application.DTOs.PublicationDTO.ValidationDTOs.PendingPublicationSearchParamsDTO;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/admin/publications/")]
    public class ValidationController : BaseController
    {
        private readonly IApprovalService _approvalService;

        public ValidationController(IApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        #region Validate Publication

        [HttpPatch("pending/{publicationId}/validate")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> ApprovePublication(
            int publicationId,
            [FromBody] ApprovalActionDTO approvalDto
        )
        {
            int parsedUserId = GetUserIdFromToken();
            Log.Information(
                "Action: {Action} on Publication ID: {PublicationId} by Admin User ID: {AdminUserId}",
                approvalDto.Action,
                publicationId,
                parsedUserId
            );
            var approvalResult = await _approvalService.UpdatePublication(
                parsedUserId,
                publicationId,
                approvalDto
            );
            return Ok(
                new GenericResponse<PublicationApprovalResultDTO>(
                    "Operación realizada con éxito",
                    approvalResult
                )
            );
        }
        #endregion
        #region Get Publications
        [HttpGet("pending")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> GetPendingPublicationsForApproval(
            [FromQuery] PendingPublicationSearchParamsDTO searchParamsDTO
        )
        {
            int adminId = GetUserIdFromToken();
            var publications = await _approvalService.GetPendingPublicationsAsync(
                adminId,
                searchParamsDTO
            );
            return Ok(
                new GenericResponse<PublicationsAwaitingApprovalDTO>(
                    "Publicaciones pendientes obtenidas con éxito",
                    publications
                )
            );
        }

        [HttpGet("pending/{publicationId}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> GetPublicationDetailForValidation(int publicationId)
        {
            int adminId = GetUserIdFromToken();
            var publicationDetail = await _approvalService.GetPublicationDetailsAsync(
                adminId,
                publicationId
            );
            return Ok(
                new GenericResponse<PublicationDetailsForApprovalDTO>(
                    "Detalle de la publicación obtenido con éxito",
                    publicationDetail
                )
            );
        }
        #endregion
    }
}
