using InteractHub.Api.Dtos.Posts;
using InteractHub.Api.Entities;
using InteractHub.Api.Hubs;
using InteractHub.Api.Repositories; // Import thư mục Repositories
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Services
{
    public class PostService : IPostService
    {
        // 1. KHÔNG DÙNG ApplicationDbContext NỮA, CHUYỂN SANG DÙNG IPostRepository
        private readonly IPostRepository _postRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IHashtagService _hashtagService;

        public PostService(IPostRepository postRepository, IHubContext<NotificationHub> hubContext, IHashtagService hashtagService)
        {
            _postRepository = postRepository;
            _hubContext = hubContext;
            _hashtagService = hashtagService;
        }

        public async Task<object> GetAllPostAsync(string? keyword, int pageNumber, int pageSize, string? currentUserId)
        {
            var totalItems = await _postRepository.CountAsync(keyword);
            var posts = await _postRepository.GetMoreDetailPostsByUserIdAsync(keyword, pageNumber, pageSize, currentUserId!);

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return new { keyword, pageNumber, pageSize, totalItems, totalPages, items = posts };
        }

        public async Task<PostDetailResponse?> GetPostByIdAsync(int id, string? currentUserId)
        {
            return await _postRepository.GetPostsByUserIdAndIdAsync(id, currentUserId!);
        }

        public async Task<object> CreatePostAsync(CreatePostRequest request, string userId)
        {
            var post = new Post
            {
                Content = request.Content.Trim(),
                ImageUrl = request.ImageUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Dùng Repository để thêm
            await _postRepository.AddAsync(post);

            await _hashtagService.ProcessHashtagsAsync(request.Content);
            await _hubContext.Clients.All.SendAsync("ReceiveNewPost", post.Id);

            return new { message = "Post created successfully", post.Id, post.Content, post.ImageUrl, post.CreatedAt, post.UserId };
        }

        public async Task<object?> UpdatePostAsync(int id, UpdatePostRequest request, string userId)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null || post.UserId != userId) return null;

            post.Content = request.Content.Trim();
            post.ImageUrl = request.ImageUrl;
            
            await _postRepository.UpdateAsync(post);

            return new { message = "Post updated successfully", post.Id, post.Content, post.ImageUrl, post.UserId, post.CreatedAt };
        }

        public async Task<bool> DeletePostAsync(int id, string userId, bool isAdmin = false)
        {
            var post = await _postRepository.GetByIdAsync(id);

            if (post == null || (post.UserId != userId && !isAdmin)) return false;

            await _postRepository.DeleteAsync(id);
            return true;
        }

        public async Task<object> GetPostsByUserIdAsync(string userId, string? currentUserId, int pageNumber, int pageSize)
        {
            var totalItems = await _postRepository
                .GetQueryable()
                .Where(p => p.UserId == userId)
                .CountAsync();

            var posts = _postRepository.GetPostsByUserIdAsync(userId, currentUserId, pageNumber, pageSize);

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return new { pageNumber, pageSize, totalItems, totalPages, items = posts };
        }
    }
}