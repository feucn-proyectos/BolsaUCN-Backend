using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public async Task<bool> CheckOwnershipAsync(int offerorId, int publicationId)
    {
        return await _context.Publications.AnyAsync(p =>
            p.Id == publicationId && p.UserId == offerorId
        );
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
            query = query.Include(p => p.Images);

        if (options?.IncludeUser == true)
            query = query.Include(p => p.User);

        return (T?)await query.FirstOrDefaultAsync(p => p.Id == publicationId);
    }

    public async Task<(IEnumerable<Publication>, int)> GetAllPendingForApprovalAsync(
        PendingPublicationSearchParamsDTO searchParamsDTO
    )
    {
        IQueryable<Publication> query = _context
            .Publications.Where(p => p.ApprovalStatus == ApprovalStatus.EnProceso)
            .AsQueryable();

        // Filtrado
        if (!string.IsNullOrEmpty(searchParamsDTO.FilterBy))
        {
            if (searchParamsDTO.FilterBy == "Oferta")
                query = query.Where(p => p.PublicationType == PublicationType.Oferta);
            else if (searchParamsDTO.FilterBy == "CompraVenta")
                query = query.Where(p => p.PublicationType == PublicationType.CompraVenta);
        }
        // Busqueda
        if (!string.IsNullOrEmpty(searchParamsDTO.SearchTerm))
        {
            var searchTerm = searchParamsDTO.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm)
                || p.Description.ToLower().Contains(searchTerm)
            );
        }
        //Ordenamiento
        bool ascending = true; // Orden por defecto, true = asc, false = desc
        if (!string.IsNullOrEmpty(searchParamsDTO.SortOrder))
        {
            ascending = searchParamsDTO.SortOrder == "asc";
        }

        query = searchParamsDTO.SortBy switch
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
        int pageSize = searchParamsDTO.PageSize ?? _defaultPageSize;
        var publications = await query
            .Skip((searchParamsDTO.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
        return (publications, totalCount);
    }

    public async Task<(IEnumerable<Publication>, int)> GetMyPublicationsFilteredByUserIdAsync(
        int offerorId,
        MyPublicationsSeachParamsDTO searchParams
    )
    {
        IQueryable<Publication> query = _context
            .Publications.Where(p => p.UserId == offerorId)
            .AsQueryable();

        // Filtrado por estado de aprobación
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
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Aceptado);
            }
            else if (searchParams.FilterByApprovalStatus == "Rechazada")
            {
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.Rechazado);
            }
            else if (searchParams.FilterByApprovalStatus == "Pendiente")
            {
                query = query.Where(p => p.ApprovalStatus == ApprovalStatus.EnProceso);
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
}
