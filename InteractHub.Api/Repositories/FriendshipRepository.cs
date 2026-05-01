using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Friends;
using InteractHub.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly ApplicationDbContext _context;

        public FriendshipRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Friendship?> CheckFriendshipAsync(string receiverId, string requesterId)
        {
            return await _context.Friendships.FirstOrDefaultAsync(f =>
                (f.ReceiverId == receiverId && f.RequesterId == requesterId) ||
                (f.ReceiverId == requesterId && f.RequesterId == receiverId));
        }

        public async Task<Friendship?> FindPendingAsync(string currentUserId, string requesterId)
        {
            return await _context.Friendships.FirstOrDefaultAsync(f => 
                f.RequesterId == requesterId && f.ReceiverId == currentUserId && f.Status == "Pending");
        }

        public async Task<Friendship> AddAsync(Friendship fs)
        {
            await _context.Friendships.AddAsync(fs);
            await _context.SaveChangesAsync();
            return fs;
        }

        public async Task<Friendship> RemoveAsync(Friendship fs)
        {
            _context.Friendships.Remove(fs);
            await _context.SaveChangesAsync();
            return fs;
        }

        public async Task<Friendship> UpdateAsync(Friendship fs)
        {
            _context.Friendships.Update(fs);
            await _context.SaveChangesAsync();
            return fs;
        }

        public async Task<IEnumerable<FriendResponse>> GetMyFriendsAsync(string userId)
        {
            return await _context.Friendships
                .AsNoTracking()
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.Status == "Accepted")
                .Select(f => new FriendResponse
                {
                    UserId = f.RequesterId == userId ? f.ReceiverId : f.RequesterId,
                    DisplayName = f.RequesterId == userId ? f.Receiver!.DisplayName : f.Requester!.DisplayName,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<FriendResponse>> GetPendingRequestAsync(string userId)
        {
            return await _context.Friendships
                .AsNoTracking()
                .Include(f => f.Requester)
                .Where(f => f.ReceiverId == userId && f.Status == "Pending")
                .Select(f => new FriendResponse
                {
                    UserId = f.RequesterId,
                    DisplayName = f.Requester!.DisplayName,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                })
                .ToListAsync();
        }
    }
}
