using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Infrastructure.Repositories.Implements
{
    public class UserNotificationRepository : IUserNotificationRepository
    {
        private readonly AppDbContext _context;

        public UserNotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserNotification notification)
        {
            _context.UserNotifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(List<UserNotification> notifications)
        {
            _context.UserNotifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserNotification>> GetAllUnsentAsync()
        {
            return await _context.UserNotifications.Where(n => !n.IsSent).ToListAsync();
        }
    }
}
