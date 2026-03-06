using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Infrastructure.Repositories.Implements
{
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

        public async Task<List<AdminNotification>> GetAllUnsentAsync()
        {
            return await _context
                .AdminNotifications.Where(n => !n.IsSent)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(List<AdminNotification> notifications)
        {
            _context.AdminNotifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();
        }
    }
}
