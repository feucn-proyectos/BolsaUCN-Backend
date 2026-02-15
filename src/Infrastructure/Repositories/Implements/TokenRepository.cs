using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Infrastructure.Repositories.Implements
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _context;

        public TokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Whitelist> AddToWhitelistAsync(Whitelist whitelistEntry)
        {
            await _context.Whitelists.AddAsync(whitelistEntry);
            await _context.SaveChangesAsync();
            return whitelistEntry;
        }

        public async Task<bool> RemoveAllFromWhitelistByUserIdAsync(int userId)
        {
            var tokens = await _context.Whitelists.Where(w => w.UserId == userId).ToListAsync();
            _context.Whitelists.RemoveRange(tokens);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ExistsByUserIdAsync(int userId)
        {
            return await _context.Whitelists.AnyAsync(w => w.UserId == userId);
        }

        public async Task<bool> IsTokenWhitelistedAsync(int userId, string token)
        {
            return await _context.Whitelists.AnyAsync(w => w.UserId == userId && w.Token == token);
        }
    }
}
