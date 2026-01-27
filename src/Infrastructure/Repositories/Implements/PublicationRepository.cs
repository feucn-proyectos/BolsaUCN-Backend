using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Resend;

public class PublicationRepository : IPublicationRepository
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly int _defaultPageSize;

    public PublicationRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _defaultPageSize = configuration.GetValue<int>("Pagination:DefaultPageSize");
    }

    public async Task<IEnumerable<Publication>> GetPublishedPublicationsByUserIdAsync(string userId)
    {
        return await _context
            .Publications.Where(p =>
                p.UserId == int.Parse(userId) && p.ApprovalStatus == ApprovalStatus.Aceptado
            ) // <-- Filtro Published
            .AsNoTracking()
            .ToListAsync();
    }

    // --- IMPLEMENTACIÓN REJECTED ---
    public async Task<IEnumerable<Publication>> GetRejectedPublicationsByUserIdAsync(string userId)
    {
        return await _context
            .Publications.Where(p =>
                p.UserId == int.Parse(userId) && p.ApprovalStatus == ApprovalStatus.Rechazado
            ) // <-- Filtro Rejected
            .AsNoTracking()
            .ToListAsync();
    }

    // --- IMPLEMENTACIÓN PENDING ("InProcess") ---
    public async Task<IEnumerable<Publication>> GetPendingPublicationsByUserIdAsync(string userId)
    {
        return await _context
            .Publications.Where(p =>
                p.UserId == int.Parse(userId) && p.ApprovalStatus == ApprovalStatus.EnProceso
            ) // <-- Filtro Pending
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Publication?> GetByIdAsync(int id)
    {
        return await _context.Publications.FirstOrDefaultAsync(p => p.Id == id);
    }

    // --- NUEVA IMPLEMENTACIÓN: UpdateAsync ---
    public async Task UpdateAsync(Publication publication)
    {
        _context.Publications.Update(publication);
        await _context.SaveChangesAsync();
    }

    public async Task<Publication?> GetPublicationByIdAsync(
        int publicationId,
        PublicationQueryOptions options
    )
    {
        var query = _context.Publications.AsQueryable();

        if (!options.TrackChanges)
            query = query.AsNoTracking();

        if (options.IncludeImages)
            query = query.Include(p => p.Images);

        if (options.IncludeUser)
            query = query.Include(p => p.User);

        return await query.FirstOrDefaultAsync(p => p.Id == publicationId);
    }

    public async Task<(IEnumerable<Publication>, int)> GetAllPendingForApprovalAsync(
        SearchParamsDTO searchParamsDTO
    )
    {
        var query = _context
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
}
