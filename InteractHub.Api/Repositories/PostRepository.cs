using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Posts;
using InteractHub.Api.Entities;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace InteractHub.Api.Repositories
{
    public class PostRepository : IPostRepository
    {
        // Đây là nơi duy nhất được phép gọi thẳng vào DbContext
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _context.Posts
                .Include(p => p.User) // Lấy luôn thông tin User đăng bài
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Post> AddAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<Post> UpdateAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountAsync(string? keyword)
        {

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                return await _context.Posts
                    .AsNoTracking()
                    .Where(p => p.Content.Contains(keyword))
                    .CountAsync();
            }

            return await _context.Posts.CountAsync();
        }

        public async Task<int> CountUserAsync(string id)
        {
            return await _context.Posts.CountAsync(p => p.UserId == id);
        }

        public async Task<PostDetailResponse?> GetPostsByUserIdAndIdAsync(int id, string? userId)
        {
            return await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.Id == id)
                .Select(p => new PostDetailResponse
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserDisplayName = p.User.DisplayName,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count,
                    IsLiked = userId != null && p.Likes.Any(l => l.UserId == userId),
                    Comments = p.Comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserId = c.UserId,
                        UserDisplayName = c.User.DisplayName
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PostDetailResponse>> GetPostsByUserIdAsync(string userId, string? currentUserId, int pageNumber, int pageSize)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostDetailResponse
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserDisplayName = p.User.DisplayName,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count,
                    IsLiked = currentUserId != null && p.Likes.Any(l => l.UserId == currentUserId),
                    Comments = p.Comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserId = c.UserId,
                        UserDisplayName = c.User.DisplayName
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PostDetailResponse>> GetMoreDetailPostsByUserIdAsync(string? keyword, int pageNumber, int pageSize, string? currentUserId)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                return await _context.Posts
                .AsNoTracking()
                .Where(p => p.Content.Contains(keyword!))
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostDetailResponse
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserDisplayName = p.User.DisplayName,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count,
                    IsLiked = currentUserId != null && p.Likes.Any(l => l.UserId == currentUserId),
                    Comments = p.Comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserId = c.UserId,
                        UserDisplayName = c.User.DisplayName
                    }).ToList()
                })
                .ToListAsync();
            }


            return await _context.Posts
                .AsNoTracking()
                //.Where(p => p.Content.Contains(keyword!))
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostDetailResponse
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserDisplayName = p.User.DisplayName,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count,
                    IsLiked = currentUserId != null && p.Likes.Any(l => l.UserId == currentUserId),
                    Comments = p.Comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserId = c.UserId,
                        UserDisplayName = c.User.DisplayName
                    }).ToList()
                })
                .ToListAsync();
        }


        public IQueryable<Post> GetQueryable()
        {
            return _context.Posts.AsQueryable();
        }
    }
}