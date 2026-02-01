using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/publications")]
    public class BuySellController : BaseController
    {
        private readonly IPublicationService _publicationService;

        public BuySellController(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        // TODO: REIMPLEMENT SOLUTION AS NOW ITS A COPY OF PUBLICATION CONTROLLER METHOD
        /// <summary>
        /// Crea una nueva publicación de compra/venta
        /// </summary>
        /// <param name="buySellDto">Datos de la publicación de compra/venta a crear</param>
        /// <returns>Resultado de la creación de la publicación de compra/venta con el ID generado</returns>
        public async Task<IActionResult> CreateBuySell([FromBody] CreateBuySellDTO buySellDto)
        {
            int parsedUserId = GetUserIdFromToken();
            var result = await _publicationService.CreateBuySellAsync(buySellDto, parsedUserId);
            return Ok(
                new GenericResponse<string>(
                    "Publicación de compra/venta creada exitosamente.",
                    result
                )
            );
        }
    }
}
