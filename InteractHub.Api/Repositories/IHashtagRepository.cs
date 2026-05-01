using InteractHub.Api.Entities;

namespace InteractHub.Api.Repositories
{
    public interface IHashtagRepository
    {
        Task<Hashtag?> GetHashtagByNameAsync(string name);
        Task<IEnumerable<Hashtag>> GetTrendingHashtagAsync(int limit); 
        Task<Hashtag> AddAsync(Hashtag h);
    }
}
