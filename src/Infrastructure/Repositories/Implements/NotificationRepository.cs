using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// EF Core implementation of <see cref="INotificationRepository"/>.
    /// Stores and retrieves notifications from the application's database.
    /// Note: this implementation currently operates on <see cref="NotificationDTO"/> objects.
    /// In a conventional design you may map DTOs to domain entities before persisting.
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(NotificationDTO notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<NotificationDTO>> GetByUserEmailAsync(string email)
        {
            return await _context
                .Notifications.Where(n => n.UserEmail == email)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
