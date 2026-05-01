using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Stories;
using InteractHub.Api.Entities;
using InteractHub.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Services
{
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _storyRepository;

        public StoryService(IStoryRepository storyRepository)
        {
            _storyRepository = storyRepository;
        }

        public async Task<object> CreateStoryAsync(CreateStoryRequest request, string userId)
        {
            var story = new Story
            {
                UserId = userId,
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddHours(24),
            };

            await _storyRepository.AddAsync(story);

            return new
            {
                message = "Story created successfully.",
                story.Id,
                userId,
                story.ImageUrl,
                story.CreatedAt,
                story.ExpiredAt,
            };
        }
        public async Task<IEnumerable<StoryResponse>> GetActiveStoriesAsync()
        {
            return await _storyRepository.GetActiveStoryAsync();
                
        }
        public async Task<bool> DeleteStoryAsync(int id, string userId)
        {
            var story = await _storyRepository.GetStoryByIdAsync(id);
            if (story is null || story.UserId != userId)
                return false;

            await _storyRepository.RemoveAsync(story);
            return true;
        }
    }
}
