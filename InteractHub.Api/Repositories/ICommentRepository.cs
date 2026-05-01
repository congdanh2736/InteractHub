using InteractHub.Api.Data;
using InteractHub.Api.Entities;

namespace InteractHub.Api.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> AddAsync(Comment comment);
        Task<Comment> RemoveAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment);
        Task<Comment?> FindCommentByIdAsync(int id);

    }
}
