using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Posts;
using InteractHub.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;

        public UserService(IUserRepository userRepository, IPostRepository postRepository)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public async Task<object?> GetUserProfileAsync(string userId)
        {
            var user = await _userRepository.FindUserAsync(userId);

            if (user is null)
            {
                return null;
            }

            var totalPosts = await _postRepository.CountUserAsync(userId);

            return new
            {
                user.Id,
                user.DisplayName,
                user.Email,
                totalPosts
            };
        }

        //public async Task<object?> GetPostsByUserIdAsync(string userId)
        //{
        //    var userExists = await _userRepository.FindUserAsync(userId);
        //    if (userExists is null)
        //    {
        //        return null;
        //    }

        //    return _postRepository.GetPostsByUserIdAsync(userId);
        //}

        public async Task<IEnumerable<object>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Enumerable.Empty<object>();
            }

            searchTerm = searchTerm.ToLower();

            return await _userRepository.SearchUsersAsync(searchTerm);
        }

        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }
    }
}
