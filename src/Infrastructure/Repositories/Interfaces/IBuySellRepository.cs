using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Domain.Models;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de publicaciones de compra/venta
    /// </summary>
    public interface IBuySellRepository
    {
        /// <summary>
        /// Crea una nueva publicación de compra/venta
        /// </summary>
        Task<int> CreateBuySellAsync(BuySell buySell);

        /// <summary>
        /// Obtiene todas las publicaciones de compra/venta activas
        /// </summary>
        Task<IEnumerable<BuySell>> GetAllActiveAsync();

        /// <summary>
        /// Obtiene todas las publicaciones de compra/venta pendientes de aprobación
        /// </summary>
        Task<IEnumerable<BuySell>> GetAllPendingBuySellsAsync();

        /// <summary>
        /// Obtiene todas las publicaciones de compra/venta ya publicadas
        /// </summary>
        Task<IEnumerable<BuySell>> GetPublishedBuySellsAsync();

        /// <summary>
        /// Obtiene una publicación de compra/venta por su ID
        /// </summary>
        Task<BuySell?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las publicaciones de compra/venta de un usuario
        /// </summary>
        Task<IEnumerable<BuySell>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Actualiza una publicación de compra/venta
        /// </summary>
        Task<BuySell> UpdateAsync(BuySell buySell);

        /// <summary>
        /// Elimina (desactiva) una publicación de compra/venta
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Busca publicaciones por categoría
        /// </summary>
        Task<IEnumerable<BuySell>> SearchByCategoryAsync(string category);

        /// <summary>
        /// Busca publicaciones por rango de precio
        /// </summary>
        Task<IEnumerable<BuySell>> SearchByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    }
}
