using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Friends;
using InteractHub.Api.Entities;
using InteractHub.Api.Hubs;
using InteractHub.Api.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Services
{
    public class FriendService : IFriendService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FriendService(
            IUserRepository userRepository,
            IFriendshipRepository friendshipRepository,
            INotificationRepository notificatinRepository,
            IHubContext<NotificationHub> hubContext)
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

            var requester = await _userRepository.FindUserAsync(requesterId);

            var notification = new Notification
            {
                UserId = receiverId, // Người nhận lời mời
                SenderId = requesterId, // Người gửi lời mời
                Message = $"{requester?.DisplayName} đã gửi cho bạn một lời mời kết bạn.",
                Type = "FriendRequest",
                CreatedAt = DateTime.UtcNow,
            };
            await _notificationRepository.AddNotificationAsync(notification);

            // Bắn signalR cho người nhận (Yêu cầu tiêm _hubContext)
            await _hubContext.Clients.User(receiverId).SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Message,
                notification.Type,
                notification.SenderId,
                notification.CreatedAt,
            });

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
