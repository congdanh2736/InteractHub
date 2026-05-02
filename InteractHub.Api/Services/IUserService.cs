namespace InteractHub.Api.Services
{
    public interface IUserService
    {
        Task<object?> GetUserProfileAsync(string userId);
        //Task<object?> GetPostsByUserIdAsync(string userId);

        Task<IEnumerable<object>> SearchUsersAsync(string searchTerm);

        Task<IEnumerable<object>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(string id);
    }
}
