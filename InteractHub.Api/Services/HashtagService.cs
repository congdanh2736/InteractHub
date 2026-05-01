using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Hashtags;
using InteractHub.Api.Entities;
using InteractHub.Api.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace InteractHub.Api.Services
{
    public class HashtagService : IHashtagService
    {
        private readonly IHashtagRepository _hashtagRepository;

        public HashtagService (IHashtagRepository hashtagRepository)
        {
            _hashtagRepository = hashtagRepository;
        }

        public async Task<object?> CreateHashtagAsync(HashtagDtos request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return null;

            var existingTag = await _hashtagRepository.GetHashtagByNameAsync(request.Name);

            if (existingTag is not null) return null;

            var newTag = new Hashtag
            {
                Name = request.Name,
                UsageCount = 0,
            };

            await _hashtagRepository.AddAsync(newTag);

            return new
            {
                message = "Hashtag created successfully.",
                newTag.Id,
                newTag.Name,
                newTag.UsageCount
            };
        }

        public async Task ProcessHashtagsAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            var regex = new Regex(@"#\w+");
            var matches = regex.Matches(content);

            var tags = matches
                .Select(m => m.Value.ToLower())
                .Distinct()
                .ToList();

            foreach (var tag in tags)
            {
                var existingTag = await _hashtagRepository.GetHashtagByNameAsync(tag);
                if (existingTag != null)
                {
                    existingTag.UsageCount += 1;   
                }
                else
                {
                    await _hashtagRepository.AddAsync(new Hashtag
                    {
                        Name = tag,
                        UsageCount = 1
                    });
                }
            }
        }

        public async Task<IEnumerable<Hashtag>> GetTrendingAsync(int limit = 5)
        {
            return await _hashtagRepository.GetTrendingHashtagAsync(limit);
        }
    }
}
