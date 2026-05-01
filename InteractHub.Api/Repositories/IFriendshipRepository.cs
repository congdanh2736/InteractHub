using InteractHub.Api.Dtos.Friends;
using InteractHub.Api.Entities;

namespace InteractHub.Api.Repositories
{
    public interface IFriendshipRepository
    {
        Task<Friendship?> CheckFriendshipAsync(string receiverId, string requesterId);
        Task<Friendship> AddAsync(Friendship fs);
        Task<Friendship> RemoveAsync(Friendship fs);
        Task<Friendship> UpdateAsync(Friendship fs);
        Task<Friendship?> FindPendingAsync(string currentUserId, string requesterId);

        Task<IEnumerable<FriendResponse>> GetMyFriendsAsync(string userId);
        Task<IEnumerable<FriendResponse>> GetPendingRequestAsync(string userId);
    }
}
