using backend.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Infrastructure.Repositories.Implements;

public class AdminNotificationRepository : IAdminNotificationRepository
{
    private readonly AppDbContext _context;

    public AdminNotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AdminNotification notification)
    {
        _context.AdminNotifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AdminNotification>> GetAllAsync()
    {
        return await _context.AdminNotifications.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<AdminNotification?> GetByIdAsync(int id)
    {
        return await _context.AdminNotifications.FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task UpdateAsync(AdminNotification notification)
    {
        _context.AdminNotifications.Update(notification);
        await _context.SaveChangesAsync();
    }
}
