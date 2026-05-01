using InteractHub.Api.Data;
using InteractHub.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Repositories
{
    public class HashtagRepository : IHashtagRepository
    {
        private readonly ApplicationDbContext _context;

        public HashtagRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Hashtag?> GetHashtagByNameAsync(string name)
        {
            return await _context.Hashtags.FirstOrDefaultAsync(h => h.Name == name);
        }

        public async Task<IEnumerable<Hashtag>> GetTrendingHashtagAsync(int limit)
        {
            return await _context.Hashtags
                .AsNoTracking()
                .OrderByDescending(h => h.UsageCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Hashtag> AddAsync(Hashtag h)
        {
            await _context.Hashtags.AddAsync(h);
            await _context.SaveChangesAsync();
            return h;
        }
    }
}
