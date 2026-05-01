using InteractHub.Api.Dtos.Users;
using InteractHub.Api.Entities;

namespace InteractHub.Api.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> FindUserAsync(string id);
        Task<IEnumerable<UserDtos>> SearchUsersAsync(string searchTerm);
    }
}
