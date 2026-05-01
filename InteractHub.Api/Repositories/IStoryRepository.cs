using InteractHub.Api.Dtos.Stories;
using InteractHub.Api.Entities;

namespace InteractHub.Api.Repositories
{
    public interface IStoryRepository
    {
        Task<Story> AddAsync(Story story);
        Task<Story> RemoveAsync(Story story);
        Task<IEnumerable<StoryResponse>> GetActiveStoryAsync();
        Task<Story?> GetStoryByIdAsync(int id);
    }
}
