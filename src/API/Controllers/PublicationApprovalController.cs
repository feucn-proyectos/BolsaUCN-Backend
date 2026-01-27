using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SearchParamsDTO = backend.src.Application.DTOs.PublicationDTO.ValidationDTOs.SearchParamsDTO;

namespace backend.src.API.Controllers
{
    [Route("api/publications/")]
    public class ValidationController : BaseController
    {
        private readonly IApprovalService _approvalService;

        public ValidationController(IApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        #region Validate Publication

        /// <summary>
        /// Valida o rechaza una publicación (oferta o compra/venta) según la acción indicada
        /// </summary>
        /// <param name="publicationId">ID de la publicación a validar</param>
        /// <param name="type">Tipo de publicación (oferta o compra/venta)</param>
        /// <param name="action">Acción a realizar (publicar o rechazar)</param>
        /// <returns>Respuesta indicando si la validación fue exitosa con la ID de la publicación</returns>
        /**
        *? TO REPLACE VALIDATION METHODS IN PublicationController
        *? PATCH PublishBuySell
        *? PATCH RejectBuySell
        *? PATCH PublishOffer
        *? PATCH RejectOffer
        *DONE Refactor Frontend to use this single endpoint for publication validation as it currently constructs the URL dynamically and passes the action and type by query parameters
        *? see validationService.tsx for reference
        *DONE Change return type from generic object to something more specific
        */
        [HttpPatch("{publicationId}/validate")]
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
                approvalDto.Action
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
            [FromQuery] SearchParamsDTO searchParamsDTO
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
