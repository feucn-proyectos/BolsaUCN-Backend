using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace backend.src.Infrastructure.Repositories.Implements;

/// <summary>
/// Implementación del repositorio para operaciones de Offer en la base de datos
/// </summary>
public class OfferRepository : IOfferRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<OfferRepository> _logger;

    public OfferRepository(AppDbContext context, ILogger<OfferRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las ofertas activas que aún no han expirado
    /// </summary>
    public async Task<IEnumerable<Offer>> GetAllActiveAsync()
    {
        _logger.LogInformation("Consultando ofertas activas en la base de datos");
        var offers = await _context
            .Offers.Include(o => o.User)
            .Where(o => o.IsOpen && o.EndDate > DateTime.UtcNow)
            .AsNoTracking()
            .ToListAsync();
        _logger.LogInformation(
            "Consulta completada: {Count} ofertas activas encontradas",
            offers.Count
        );
        return offers;
    }

    /// <summary>
    /// Obtiene todas las ofertas que están pendientes de validación
    /// </summary>
    public async Task<IEnumerable<Offer>> GetAllPendingOffersAsync()
    {
        _logger.LogInformation("Consultando ofertas pendientes en la base de datos");
        var offers = await _context
            .Offers.Include(o => o.User)
            .Where(o => o.ApprovalStatus == ApprovalStatus.EnProceso)
            .AsNoTracking()
            .ToListAsync();
        _logger.LogInformation(
            "Consulta completada: {Count} ofertas pendientes encontradas",
            offers.Count
        );
        return offers;
    }

    /// <summary>
    /// Obtiene todas las ofertas que han sido publicadas
    /// </summary>
    public async Task<IEnumerable<Offer>> PublishedOffersAsync()
    {
        _logger.LogInformation("Consultando ofertas publicadas en la base de datos");
        var offers = await _context
            .Offers.Include(o => o.User)
            .Where(o => o.ApprovalStatus == ApprovalStatus.Aceptado && o.IsOpen == true)
            .AsNoTracking()
            .ToListAsync();
        _logger.LogInformation(
            "Consulta completada: {Count} ofertas publicadas encontradas",
            offers.Count
        );
        return offers;
    }

    /// <summary>
    /// Obtiene una oferta por su ID con sus relaciones cargadas
    /// </summary>
    public async Task<Offer?> GetByIdAsync(int offerId)
    {
        var offer = await _context
            .Offers.Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == offerId);
        return offer;
    }

    /// <summary>
    /// Crea una nueva oferta en la base de datos
    /// </summary>
    public async Task<int> CreateOfferAsync(Offer offer)
    {
        Log.Information(
            "Creando nueva oferta: {Title} para usuario ID: {UserId}",
            offer.Title,
            offer.UserId
        );
        _context.Offers.Add(offer);
        await _context.SaveChangesAsync();
        return offer.Id;
    }

    /// <summary>
    /// Obtiene una oferta por ID incluyendo sus imágenes
    /// </summary>
    public async Task<Offer?> GetOfferByIdAsync(int id)
    {
        _logger.LogInformation("Consultando oferta por ID: {OfferId}", id);
        var offer = await _context
            .Offers.Include(o => o.User)
            .Include(o => o.Images)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer != null)
        {
            _logger.LogInformation("Oferta ID: {OfferId} encontrada", id);
        }
        else
        {
            _logger.LogWarning("Oferta ID: {OfferId} no encontrada", id);
        }
        return offer;
    }

    /// <summary>
    /// Obtiene todas las ofertas del sistema
    /// </summary>
    public async Task<IEnumerable<Offer>> GetAllOffersAsync()
    {
        _logger.LogInformation("Consultando todas las ofertas en la base de datos");
        var offers = await _context.Offers.Include(o => o.User).AsNoTracking().ToListAsync();
        _logger.LogInformation("Consulta completada: {Count} ofertas encontradas", offers.Count);
        return offers;
    }

    /// <summary>
    /// Obtiene todas las ofertas creadas por un usuario específico (empresa/individual)
    /// </summary>
    public async Task<IEnumerable<Offer>> GetOffersByUserIdAsync(int userId)
    {
        _logger.LogInformation("Consultando ofertas del usuario ID: {UserId}", userId);
        var offers = await _context
            .Offers.Include(o => o.User)
            .Where(o => o.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
        _logger.LogInformation("Usuario ID: {UserId} tiene {Count} ofertas", userId, offers.Count);
        return offers;
    }

    /// <summary>
    /// Actualiza una oferta existente
    /// </summary>
    public async Task<bool> UpdateOfferAsync(Offer offer)
    {
        try
        {
            _logger.LogInformation("Actualizando oferta ID: {OfferId}", offer.Id);
            _context.Offers.Update(offer);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("Oferta ID: {OfferId} actualizada exitosamente", offer.Id);
                return true;
            }

            _logger.LogWarning("No se pudo actualizar la oferta ID: {OfferId}", offer.Id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la oferta ID: {OfferId}", offer.Id);
            throw new Exception("Error al actualizar la oferta", ex);
        }
    }

    /// <summary>
    /// Elimina una oferta de la base de datos
    /// </summary>
    public async Task<bool> DeleteOfferAsync(int id)
    {
        try
        {
            _logger.LogInformation("Intentando eliminar oferta ID: {OfferId}", id);
            var offer = await _context.Offers.FindAsync(id);

            if (offer == null)
            {
                _logger.LogWarning("Oferta ID: {OfferId} no encontrada para eliminación", id);
                return false;
            }

            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Oferta ID: {OfferId} eliminada exitosamente", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la oferta ID: {OfferId}", id);
            throw new Exception("Error al eliminar la oferta", ex);
        }
    }
}
