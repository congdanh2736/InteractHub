using InteractHub.Api.Dtos.Hashtags;
using InteractHub.Api.Entities;

namespace InteractHub.Api.Services
{
    public interface IHashtagService
    {
        Task ProcessHashtagsAsync(string content);
        Task<IEnumerable<Hashtag>> GetTrendingAsync(int limit = 5);
        Task<object?> CreateHashtagAsync(HashtagDtos request);
    }
}
