using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Stories;
using InteractHub.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Repositories
{
    public class StoryRepository : IStoryRepository
    {
        private readonly ApplicationDbContext _context;

        public StoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Story> AddAsync(Story story)
        {
            await _context.Stories.AddAsync(story);
            await _context.SaveChangesAsync();
            return story;
        }

        public async Task<Story> RemoveAsync(Story story)
        {
            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();
            return story;
        }

        public async Task<IEnumerable<StoryResponse>> GetActiveStoryAsync()
        {
            return await _context.Stories
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.ExpiredAt > DateTime.UtcNow) // lay story con han
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new StoryResponse
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserDisplayName = s.User != null ? s.User.DisplayName : "Unknown",
                    ImageUrl = s.ImageUrl,
                    CreatedAt = s.CreatedAt,
                    ExpiredAt = s.ExpiredAt
                })
                .ToListAsync();
        }

        public async Task<Story?> GetStoryByIdAsync(int id)
        {
            return await _context.Stories.FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
