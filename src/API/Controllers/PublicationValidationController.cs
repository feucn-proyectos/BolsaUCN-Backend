using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.API.Controllers
{
    [Route("api/admin")]
    public class ValidationController : BaseController
    {
        private readonly IValidationService _validationService;

        public ValidationController(IValidationService validationService)
        {
            _validationService = validationService;
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
        *TODO Refactor Frontend to use this single endpoint for publication validation
        *TODO Change return type from generic object to something more specific
        */
        [HttpPatch(
            "publications/{type:regex(^(offer|buySell)$)}/{publicationId:int}/{action:regex(^(publish|reject)$)}"
        )]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> ValidatePublication(
            int publicationId,
            string type,
            string action
        )
        {
            int parsedUserId = GetUserIdFromToken();
            var validationResult = await _validationService.ValidatePublication(
                parsedUserId,
                publicationId,
                type,
                action
            );
            return Ok(new GenericResponse<object>("Operación realizada con éxito", publicationId));
        }
        #endregion
    }
}
