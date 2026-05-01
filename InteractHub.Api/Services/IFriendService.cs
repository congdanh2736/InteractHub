using InteractHub.Api.Dtos.Friends;

namespace InteractHub.Api.Services
{
    public interface IFriendService
    {
        Task<object?> SendFriendRequestAsync(string requesterId, string receiverId);
        Task<object?> RespondToRequestAsync(string currentUserId, string requesterId, string status);
        Task<IEnumerable<FriendResponse>> GetMyFriendAsync(string userId);
        Task<IEnumerable<FriendResponse>> GetPendingRequestAsync(string userId);
        Task<object?> GetFriendshipStatusAsync(string currentUserId, string targetUserId);
        Task<bool> UnfriendAsync(string currentUserId, string targetUserId);
    }
}
