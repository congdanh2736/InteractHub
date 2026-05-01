using InteractHub.Api.Data;
using InteractHub.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly ApplicationDbContext _context;

        public LikeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Like?> GetLikeByIdAsync(int postId, string userId)
        {
            return await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<Like> RemoveAsync(Like like)
        {
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task<Like> AddAsync(Like like)
        {
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();
            return like;
        }
    }
}
