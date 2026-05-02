using InteractHub.Api.Dtos.Posts;

namespace InteractHub.Api.Services
{
    public interface IPostService
    {
        Task<object> GetAllPostAsync(string? keyword, int pageNumber, int pageSize, string? currentUserId);
        Task<PostDetailResponse?> GetPostByIdAsync(int id, string? currentUserId);
        Task<object> CreatePostAsync(CreatePostRequest request, string userId);
        Task<object?> UpdatePostAsync(int id, UpdatePostRequest request, string userId);
        Task<bool> DeletePostAsync(int id, string userId, bool isAdmin = false);
        Task<object> GetPostsByUserIdAsync(string userId, string? currentUserId, int pageNumber, int pageSize);
    }
}
