using InteractHub.Api.Entities;

namespace InteractHub.Api.Repositories
{
    public interface ILikeRepository
    {
        Task<Like?> GetLikeByIdAsync(int postId, string userId);
        Task<Like> RemoveAsync(Like like);
        Task<Like> AddAsync(Like like);
    }
}
