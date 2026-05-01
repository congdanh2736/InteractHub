using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Friends;
using InteractHub.Api.Entities;
using InteractHub.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Services
{
    public class FriendService : IFriendService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;

        public FriendService(
            IUserRepository userRepository,
            IFriendshipRepository friendshipRepository)
        {
            _friendshipRepository = friendshipRepository;
            _userRepository = userRepository;
        }

        public async Task<object?> SendFriendRequestAsync(string requesterId, string receiverId)
        {
            if (requesterId == receiverId) return null;

            var receiverExists = await _userRepository.FindUserAsync(receiverId);
            if (receiverExists is null) return null;

            // kiểm tra đã là bạn bè hoặc đã gửi lời mời hay chưa
            var existingFriendship = await _friendshipRepository.CheckFriendshipAsync(receiverId, requesterId);
            if (existingFriendship is not null)
                return new
                {
                    message = "Friendship or request already exists."
                };

            var friendship = new Friendship
            {
                RequesterId = requesterId,
                ReceiverId = receiverId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
            };

            await _friendshipRepository.AddAsync(friendship);

            return new
            {
                message = "Friend request sent successfully."
            };
        }

        public async Task<object?> RespondToRequestAsync(string currentUserId, string requesterId, string status)
        {
            var friendship = await _friendshipRepository.FindPendingAsync(currentUserId, requesterId);
            if (friendship is null) return null;

            if (status == "Accepted")
            {
                friendship.Status = "Accepted";

                await _friendshipRepository.UpdateAsync(friendship);
            }
            else if (status == "Declined")
            {
                await _friendshipRepository.RemoveAsync(friendship);
            }
            else return null;

            return new
            {
                message = $"Friend request {status.ToLower()}."
            };
        }

        public async Task<IEnumerable<FriendResponse>> GetMyFriendAsync(string userId)
        {
            return await _friendshipRepository.GetMyFriendsAsync(userId);
        }

        public async Task<IEnumerable<FriendResponse>> GetPendingRequestAsync(string userId)
        {
            return await _friendshipRepository.GetPendingRequestAsync(userId);
        }

        public async Task<object?> GetFriendshipStatusAsync(string currentUserId, string targetUserId)
        {
            var friendship = await _friendshipRepository.CheckFriendshipAsync(currentUserId, targetUserId);
            if (friendship == null)
                return new { status = "None" }; // Chưa có quan hệ gì

            return new
            {
                status = friendship.Status, // Sẽ là "Pending" hoặc "Accepted"
                requesterId = friendship.RequesterId // Để biết ai là người đã gửi lời mời
            };
        }

        public async Task<bool> UnfriendAsync(string currentUserId, string targetUserId)
        {
            var friendship = await _friendshipRepository.CheckFriendshipAsync(currentUserId, targetUserId);
            if (friendship == null)
                return false;

            // Xóa record (Dùng cho cả hủy kết bạn và hủy bỏ lời mời đang chờ)
            await _friendshipRepository.RemoveAsync(friendship);
            return true;
        }
    }
}
