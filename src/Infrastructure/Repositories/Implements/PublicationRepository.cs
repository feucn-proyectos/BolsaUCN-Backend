using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Resend;

public class PublicationRepository : IPublicationRepository
{
    private readonly AppDbContext _context; // Reemplaza con el nombre de tu DbContext

    public PublicationRepository(AppDbContext context) // Reemplaza con tu DbContext
    {
        _context = context;
    }

    public async Task<IEnumerable<Publication>> GetPublishedPublicationsByUserIdAsync(string userId)
    {
        return await _context
            .Publications.Where(p =>
                p.UserId == int.Parse(userId) && p.StatusValidation == StatusValidation.Publicado
            ) // <-- Filtro Published
            .AsNoTracking()
            .ToListAsync();
    }

    // --- IMPLEMENTACIÓN REJECTED ---
    public async Task<IEnumerable<Publication>> GetRejectedPublicationsByUserIdAsync(string userId)
    {
        return await _context
            .Publications.Where(p =>
                p.UserId == int.Parse(userId) && p.StatusValidation == StatusValidation.Rechazado
            ) // <-- Filtro Rejected
            .AsNoTracking()
            .ToListAsync();
    }

    // --- IMPLEMENTACIÓN PENDING ("InProcess") ---
    public async Task<IEnumerable<Publication>> GetPendingPublicationsByUserIdAsync(string userId)
    {
        return await _context
            .Publications.Where(p =>
                p.UserId == int.Parse(userId) && p.StatusValidation == StatusValidation.EnProceso
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
}
