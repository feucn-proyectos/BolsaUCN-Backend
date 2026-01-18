using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.PublicationDTO;

namespace backend.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de publicaciones de compra/venta
    /// </summary>
    public interface IBuySellService
    {
        /// <summary>
        /// Obtiene todas las publicaciones de compra/venta activas
        /// </summary>
        Task<IEnumerable<BuySellSummaryDto>> GetActiveBuySellsAsync();

        /// <summary>
        /// Obtiene todas las publicaciones de compra/venta pendientes de validación
        /// </summary>
        Task<IEnumerable<BuySellSummaryDto>> GetAllPendingBuySellsAsync();

        /// <summary>
        /// Obtiene los detalles de una publicación de compra/venta específica
        /// </summary>
        Task<BuySellDetailDto?> GetBuySellDetailsAsync(int buySellId);

        /// <summary>
        /// Obtiene todas las publicaciones de compra/venta ya publicadas.
        /// </summary>
        Task<IEnumerable<BuySellBasicAdminDto>> GetPublishedBuysellsAsync();

        Task<BuySellDetailDto> GetBuySellDetailForOfferer(int id, string userId);

        /// <summary>
        /// Metodo que aprueba y publica la compra/venta, (lo utiliza el admin)
        /// </summary>
        /// <param name="buySellId"></param>
        /// <returns></returns>
        Task GetBuySellForAdminToPublish(int buySellId);

        /// <summary>
        /// Meotdo que rechaza la compra/venta, (lo utiliza el admin)
        /// </summary>
        /// <param name="buySellId"></param>
        /// <returns></returns>
        Task GetBuySellForAdminToReject(int buySellId);
        Task ClosePublishedBuySellAsync(int buySellId);

        /// <summary>
        /// Cierra (desactiva) una publicación de compra/venta. Solo el propietario puede usarlo.
        /// </summary>
        /// <param name="buySellId">Identificador de la publicación.</param>
        /// <param name="offererUserId">Identificador del usuario oferente (propietario).</param>
        Task ClosePublishedBuySellForOffererAsync(int buySellId, int offererUserId);
    }
}
