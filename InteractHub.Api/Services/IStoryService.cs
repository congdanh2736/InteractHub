using InteractHub.Api.Dtos.Stories;

namespace InteractHub.Api.Services
{
    public interface IStoryService
    {
        Task<object> CreateStoryAsync(CreateStoryRequest request, string userId);
        Task<IEnumerable<StoryResponse>> GetActiveStoriesAsync();
        Task<bool> DeleteStoryAsync(int id, string userId);
    }
}
