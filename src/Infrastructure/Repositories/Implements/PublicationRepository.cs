using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.BuySells;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.SpecificUserPublicationsDTO;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PublicationRepository : IPublicationRepository
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly int _defaultPageSize;

    public PublicationRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
    }

    public async Task<(T, bool)> CreatePublicationAsync<T>(T publication)
        where T : Publication
    {
        await _context.Set<T>().AddAsync(publication);
        int rowsAffected = await _context.SaveChangesAsync();
        return (publication, rowsAffected > 0);
    }

    public async Task<bool> CheckOwnershipAsync(int offerorId, int publicationId)
    {
        return await _context.Publications.AnyAsync(p =>
            p.Id == publicationId && p.UserId == offerorId
        );
    }

    public async Task<bool?> CheckType(int publicationId, PublicationType type)
    {
        var result = _context.Publications.AnyAsync(p =>
            p.Id == publicationId && p.PublicationType == type
        );
        return await result;
    }

    public async Task<bool> UpdateAsync<T>(T publication)
        where T : Publication
    {
        _context.Set<T>().Update(publication);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<T?> GetPublicationByIdAsync<T>(
        int publicationId,
        PublicationQueryOptions? options = null
    )
        where T : Publication
    {
        var query = _context.Set<T>().AsQueryable();

        if (options?.TrackChanges == false)
            query = query.AsNoTracking();

        if (typeof(T) == typeof(BuySell) && options?.IncludeImages == true)
            query = (IQueryable<T>)((IQueryable<BuySell>)query).Include(o => o.Images);

        if (typeof(T) == typeof(Offer) && options?.IncludeApplications == true)
            query =
                (IQueryable<T>)
                    ((IQueryable<Offer>)query)
                        .Include(o => o.Applications)
                        .ThenInclude(s => s.Student);
        if (options?.IncludeUser == true)
            query = query.Include(p => p.User);

        return await query.FirstOrDefaultAsync(p => p.Id == publicationId);
    }

    public async Task<(
        List<Publication> publications,
        int totalCount
    )> GetPublicationsByUserIdFilteredAsync(
        int userId,
        UserPublicationsSearchParamsDTO searchParams
    )
    {
        IQueryable<Publication> query = _context
            .Publications.Where(p => p.UserId == userId)
            .AsQueryable();

        // Filtrado por tipo y estado de aprobación
        if (!string.IsNullOrEmpty(searchParams.FilterByPublicationType))
        {
            if (searchParams.FilterByPublicationType == "Oferta")
            {
                query = query.Where(p => p.PublicationType == PublicationType.Oferta);
            }
            else if (searchParams.FilterByPublicationType == "CompraVenta")
            {
                query = query.Where(p => p.PublicationType == PublicationType.CompraVenta);
            }
        }
        if (!string.IsNullOrEmpty(searchParams.FilterByPublicationStatus))
        {
            if (searchParams.FilterByPublicationStatus == "Aprobada")
            {
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Aceptada);
            }
            else if (searchParams.FilterByPublicationStatus == "Rechazada")
            {
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Rechazada);
            }
            else if (searchParams.FilterByPublicationStatus == "Pendiente")
            {
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Pendiente);
            }
        }

        // Ordenamiento
        if (!string.IsNullOrEmpty(searchParams.SortBy))
        {
            query = searchParams.SortBy switch
            {
                "Title" => searchParams.SortOrder == "asc"
                    ? query.OrderBy(p => p.Title)
                    : query.OrderByDescending(p => p.Title),
                "CreatedAt" => searchParams.SortOrder == "asc"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Id),
            };
        }

        // Paginación
        int totalCount = await query.CountAsync();
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        var publications = await query
            .Skip((searchParams.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (publications, totalCount);
    }

    public async Task<(IEnumerable<Offer>?, int)> GetOffersFilteredAsync(
        ExploreOffersSearchParamsDTO searchParams,
        int? userId = null
    )
    {
        IQueryable<Offer> query = _context
            .Offers.Where(u => u.UserId != userId)
            .Where(p =>
                p.ApprovalStatus == ApprovalStatus.Aceptada
                && DateTime.UtcNow <= p.ApplicationDeadline
                && p.WorkStartedAt == null
            )
            .AsQueryable();

        // Filtrado
        if (!string.IsNullOrEmpty(searchParams.FilterBy))
        {
            if (searchParams.FilterBy == "Trabajo")
                query = query.Where(p => p.OfferType == OfferTypes.Trabajo);
            else if (searchParams.FilterBy == "Voluntariado")
                query = query.Where(p => p.OfferType == OfferTypes.Voluntariado);
        }
        // Busqueda
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            var searchTerm = searchParams.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm)
                || p.Description.ToLower().Contains(searchTerm)
            );
        }
        //Ordenamiento
        bool ascending = true; // Orden por defecto, true = asc, false = desc
        if (!string.IsNullOrEmpty(searchParams.SortOrder))
        {
            ascending = searchParams.SortOrder == "asc";
        }

        query = searchParams.SortBy switch
        {
            "Title" => ascending
                ? query.OrderBy(p => p.Title)
                : query.OrderByDescending(p => p.Title),
            "CreatedAt" => ascending
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            "Remuneration" => ascending
                ? query.OrderBy(p => p.Remuneration)
                : query.OrderByDescending(p => p.Remuneration),
            _ => query.OrderBy(p => p.Id),
        };

        // Paginacion
        int totalCount = await query.CountAsync();
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        var publications = await query
            .Skip((searchParams.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.User)
            .AsNoTracking()
            .ToListAsync();
        return (publications, totalCount);
    }

    public async Task<(IEnumerable<BuySell>?, int)> GetBuySellsFilteredAsync(
        ExploreBuySellsSearchParamsDTO searchParams,
        int? userId = null
    )
    {
        IQueryable<BuySell> query = _context
            .BuySells.Where(bs =>
                bs.UserId != userId && bs.Availability == Availability.Disponible || bs.Quantity > 0
            )
            .AsQueryable();
        // Filtrado
        // NO HAY FILTRADO POR AHORA
        // Busqueda
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            var searchTerm = searchParams.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm)
                || p.Description.ToLower().Contains(searchTerm)
            );
        }
        //Ordenamiento
        bool ascending = true; // Orden por defecto, true = asc, false = desc
        if (!string.IsNullOrEmpty(searchParams.SortOrder))
        {
            ascending = searchParams.SortOrder == "asc";
        }
        if (!string.IsNullOrEmpty(searchParams.SortBy))
        {
            query = searchParams.SortBy switch
            {
                "Title" => ascending
                    ? query.OrderBy(p => p.Title)
                    : query.OrderByDescending(p => p.Title),
                "CreatedAt" => ascending
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt),
                "Price" => ascending
                    ? query.OrderBy(p => p.Price)
                    : query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Id),
            };
        }
        // Paginacion
        int totalCount = await query.CountAsync();
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        var publications = await query
            .Skip((searchParams.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.User)
            .AsNoTracking()
            .ToListAsync();
        return (publications, totalCount);
    }

    public async Task<(IEnumerable<Publication>?, int)> GetAllPendingForApprovalAsync(
        PendingPublicationSearchParamsDTO searchParams
    )
    {
        IQueryable<Publication> query = _context
            .Publications.Where(p => p.ApprovalStatus == ApprovalStatus.Pendiente)
            .AsQueryable();

        // Filtrado
        if (!string.IsNullOrEmpty(searchParams.FilterBy))
        {
            if (searchParams.FilterBy == "Oferta")
                query = query.Where(p => p.PublicationType == PublicationType.Oferta);
            else if (searchParams.FilterBy == "CompraVenta")
                query = query.Where(p => p.PublicationType == PublicationType.CompraVenta);
        }
        // Busqueda
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            var searchTerm = searchParams.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm)
                || p.Description.ToLower().Contains(searchTerm)
            );
        }
        //Ordenamiento
        bool ascending = true; // Orden por defecto, true = asc, false = desc
        if (!string.IsNullOrEmpty(searchParams.SortOrder))
        {
            ascending = searchParams.SortOrder == "asc";
        }

        query = searchParams.SortBy switch
        {
            "Title" => ascending
                ? query.OrderBy(p => p.Title)
                : query.OrderByDescending(p => p.Title),
            "CreatedAt" => ascending
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            "CreatedBy" => ascending
                ? query.OrderBy(p => p.UserId)
                : query.OrderByDescending(p => p.UserId),
            _ => query.OrderBy(p => p.Id),
        };

        // Paginacion
        int totalCount = await query.CountAsync();
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        var publications = await query
            .Skip((searchParams.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
        return (publications, totalCount);
    }

    public async Task<(IEnumerable<Publication>?, int)> GetMyPublicationsFilteredByUserIdAsync(
        int offerorId,
        MyPublicationsSeachParamsDTO searchParams
    )
    {
        IQueryable<Publication> query = _context
            .Publications.Where(p => p.UserId == offerorId)
            .AsQueryable();

        // Filtrado por tipo y estado de aprobación
        if (!string.IsNullOrEmpty(searchParams.FilterByPublicationType))
        {
            if (searchParams.FilterByPublicationType == "Oferta")
            {
                query = query.Where(p => p.PublicationType == PublicationType.Oferta);
            }
            else if (searchParams.FilterByPublicationType == "CompraVenta")
            {
                query = query.Where(p => p.PublicationType == PublicationType.CompraVenta);
            }
        }
        if (!string.IsNullOrEmpty(searchParams.FilterByApprovalStatus))
        {
            if (searchParams.FilterByApprovalStatus == "Aprobada")
            {
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Aceptada);
            }
            else if (searchParams.FilterByApprovalStatus == "Rechazada")
            {
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Rechazada);
            }
            else if (searchParams.FilterByApprovalStatus == "Pendiente")
            {
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Pendiente);
            }
        }

        // Busqueda
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            var searchTerm = searchParams.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm)
                || p.Description.ToLower().Contains(searchTerm)
            );
        }

        //Ordenamiento
        bool ascending = true; // Orden por defecto, true = asc, false = desc
        if (!string.IsNullOrEmpty(searchParams.SortOrder))
        {
            ascending = searchParams.SortOrder == "asc";
        }

        query = searchParams.SortBy switch
        {
            "Title" => ascending
                ? query.OrderBy(p => p.Title)
                : query.OrderByDescending(p => p.Title),
            "CreatedAt" => ascending
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Id),
        };

        // Paginacion
        int totalCount = await query.CountAsync();
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        var publications = await query
            .Skip((searchParams.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
        return (publications, totalCount);
    }

    public async Task<(IEnumerable<Publication>?, int)> GetAllPublicationsFilteredForAdminAsync(
        PublicationsForAdminSearchParamsDTO searchParams
    )
    {
        IQueryable<Publication> query = _context.Publications.AsQueryable();
        // Filtrado por tipo de publicación
        if (!string.IsNullOrEmpty(searchParams.FilterByType))
        {
            if (searchParams.FilterByType == "Oferta")
                query = query.Where(p => p.PublicationType == PublicationType.Oferta);
            else if (searchParams.FilterByType == "CompraVenta")
                query = query.Where(p => p.PublicationType == PublicationType.CompraVenta);
        }
        if (!string.IsNullOrEmpty(searchParams.FilterByApprovalStatus))
        {
            if (searchParams.FilterByApprovalStatus == "Aprobada")
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Aceptada);
            else if (searchParams.FilterByApprovalStatus == "Rechazada")
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Rechazada);
            else if (searchParams.FilterByApprovalStatus == "Pendiente")
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Pendiente);
        }
        // Busqueda
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            var searchTerm = searchParams.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm)
                || p.Description.ToLower().Contains(searchTerm)
            );
        }
        //Ordenamiento
        bool ascending = true; // Orden por defecto, true = asc, false = desc
        if (!string.IsNullOrEmpty(searchParams.SortOrder))
        {
            ascending = searchParams.SortOrder == "asc";
        }
        query = searchParams.SortBy switch
        {
            "Title" => ascending
                ? query.OrderBy(p => p.Title)
                : query.OrderByDescending(p => p.Title),
            "CreatedAt" => ascending
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Id),
        };
        // Paginacion
        int totalCount = await query.CountAsync();
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        var publications = await query
            .Skip((searchParams.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(u => u.User)
            .ThenInclude(p => p.ProfilePhoto)
            .AsNoTracking()
            .ToListAsync();
        return (publications, totalCount);
    }

    public async Task RollbackCreatedBuySellAsync(int buySellId)
    {
        var buySell = await _context
            .BuySells.Include(bs => bs.Images)
            .FirstOrDefaultAsync(bs => bs.Id == buySellId);

        if (buySell != null)
        {
            // Eliminar imágenes asociadas
            if (buySell.Images != null && buySell.Images.Count != 0)
            {
                _context.Images.RemoveRange(buySell.Images);
            }

            // Eliminar la publicación de compra/venta
            _context.BuySells.Remove(buySell);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
