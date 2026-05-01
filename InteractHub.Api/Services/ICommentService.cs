using InteractHub.Api.Dtos.Comments;

namespace InteractHub.Api.Services
{
    public interface ICommentService
    {
        Task<object?> CreateCommentAsync(CreateCommentRequest request, string userId);
        Task<bool> DeleteCommentAsync(int id, string userId);
        Task<object?> UpdateCommentAsync(int id, UpdateCommentRequest request, string userId);
        Task<object?> GetCommentAsync(int id);
    }
}
