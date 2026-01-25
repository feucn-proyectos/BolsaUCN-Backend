using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Bogus.Bson;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Repositorio para gestionar publicaciones de compra/venta
    /// </summary>
    public class BuySellRepository : IBuySellRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BuySellRepository> _logger;

        public BuySellRepository(AppDbContext context, ILogger<BuySellRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CreateBuySellAsync(BuySell buySell)
        {
            _context.BuySells.Add(buySell);
            await _context.SaveChangesAsync();
            return buySell.Id;
        }

        public async Task<IEnumerable<BuySell>> GetAllActiveAsync()
        {
            return await _context
                .BuySells.Include(bs => bs.User)
                .Include(bs => bs.Images)
                .Where(bs => bs.IsOpen)
                .OrderByDescending(bs => bs.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<BuySell>> GetAllPendingBuySellsAsync()
        {
            _logger.LogInformation(
                "Consultando publicaciones de compra/venta pendientes en la base de datos"
            );
            var buysell = await _context
                .BuySells.Include(bs => bs.User)
                .Include(bs => bs.Images)
                .Where(bs => bs.StatusValidation == StatusValidation.EnProceso)
                .OrderByDescending(bs => bs.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            _logger.LogInformation(
                "Consulta completada: {Count} publicaciones de compra/venta pendientes encontradas",
                buysell.Count
            );
            return buysell;
        }

        public async Task<IEnumerable<BuySell>> GetPublishedBuySellsAsync()
        {
            _logger.LogInformation(
                "Consultando publicaciones de compra/venta publicadas en la base de datos"
            );
            var buysell = await _context
                .BuySells.Include(bs => bs.User)
                .Include(bs => bs.Images)
                .Where(bs => bs.StatusValidation == StatusValidation.Publicado)
                .OrderByDescending(bs => bs.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            _logger.LogInformation(
                "Consulta completada: {Count} publicaciones de compra/venta publicadas encontradas",
                buysell.Count
            );
            return buysell;
        }

        public async Task<BuySell?> GetByIdAsync(int id)
        {
            return await _context
                .BuySells.Include(bs => bs.User)
                .Include(bs => bs.Images)
                .FirstOrDefaultAsync(bs => bs.Id == id);
        }

        public async Task<IEnumerable<BuySell>> GetByUserIdAsync(int userId)
        {
            return await _context
                .BuySells.Where(bs => bs.UserId == userId)
                .Include(bs => bs.Images)
                .OrderByDescending(bs => bs.CreatedAt)
                .ToListAsync();
        }

        public async Task<BuySell> UpdateAsync(BuySell buySell)
        {
            try
            {
                _context.BuySells.Update(buySell);
                await _context.SaveChangesAsync();
                return buySell;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar la publicación de compra/venta", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var buySell = await GetByIdAsync(id);
                if (buySell == null)
                    return false;

                buySell.IsOpen = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar la publicación de compra/venta", ex);
            }
        }

        public async Task<IEnumerable<BuySell>> SearchByCategoryAsync(string category)
        {
            return await _context
                .BuySells.Where(bs =>
                    bs.IsOpen && bs.Category.ToLower().Contains(category.ToLower())
                )
                .Include(bs => bs.User)
                .Include(bs => bs.Images)
                .OrderByDescending(bs => bs.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BuySell>> SearchByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice
        )
        {
            return await _context
                .BuySells.Where(bs => bs.IsOpen && bs.Price >= minPrice && bs.Price <= maxPrice)
                .Include(bs => bs.User)
                .Include(bs => bs.Images)
                .OrderBy(bs => bs.Price)
                .ToListAsync();
        }
    }
}
