using InteractHub.Api.Dtos.Posts;
using InteractHub.Api.Entities;

namespace InteractHub.Api.Repositories
{
    public interface IPostRepository
    {
        // Khai báo các hành động tương tác với Database
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int id);
        Task<Post> AddAsync(Post post);
        Task<Post> UpdateAsync(Post post);
        Task<bool> DeleteAsync(int id);
        Task<int> CountAsync(string? keyword);
        Task<int> CountUserAsync(string id);
        Task<int> CountPostByUserIdAsync(string id);

        Task<PostDetailResponse?> GetPostsByUserIdAndIdAsync(int id, string? userId);
        Task<IEnumerable<PostDetailResponse>> GetPostsByUserIdAsync(string userId, string? currentUserId, int pageNumber, int pageSize);
        Task<IEnumerable<PostDetailResponse>> GetMoreDetailPostsByUserIdAsync(string? keyword, int pageNumber, int pageSize, string? currentUserId);

        IQueryable<Post> GetQueryable();

        // Dùng khi cần lưu nhiều thay đổi cùng lúc
        //Task SaveChangesAsync();
    }
}
