using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Comments;
using InteractHub.Api.Entities;
using Microsoft.EntityFrameworkCore;

// Thư viện gửi thông báo cho người dùng rằng có người đã comment
using InteractHub.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using InteractHub.Api.Repositories;

namespace InteractHub.Api.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;

        public CommentService(
            IHubContext<NotificationHub> hubContext, 
            ICommentRepository commentRepository, 
            IPostRepository postRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _hubContext = hubContext;
        }

        public async Task<object?> CreateCommentAsync(CreateCommentRequest request, string userId)
        {
            var post = await _postRepository.GetByIdAsync(request.PostId);
            if (post is null) return null;

            var commenter = await _userRepository.FindUserAsync(userId);

            var comment = new Comment
            {
                PostId = request.PostId,
                UserId = userId,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
            };

            await _commentRepository.AddAsync(comment);

            if (post.UserId != userId)
            {
                var notification = new Notification
                {
                    UserId = post.UserId,
                    Message = $"{commenter?.DisplayName} đã bình luận về bài viết của bạn.",
                    Type = "Comment",
                    CreatedAt = DateTime.UtcNow,
                };
                await _notificationRepository.AddNotificationAsync(notification);

                // bắn signal cho frontend
                await _hubContext.Clients.Users(post.UserId).SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Message,
                    notification.Type,
                    notification.CreatedAt,
                });
            }

            await _hubContext.Clients.All.SendAsync("ReceiveNewComment", request.PostId);

            return new { 
                message = "Comment created successfully.", 
                comment.Id, 
                comment.Content, 
                comment.PostId, 
                comment.UserId, 
                comment.CreatedAt 
            };
        }

        public async Task<bool> DeleteCommentAsync(int id, string userId)
        {
            var comment = await _commentRepository.FindCommentByIdAsync(id);
            if (comment == null || comment.UserId != userId) 
                return false;

            await _commentRepository.RemoveAsync(comment);
            return true;
        }

        public async Task<object?> UpdateCommentAsync(int id, UpdateCommentRequest request, string userId)
        {
            var comment = await _commentRepository.FindCommentByIdAsync(id);
            if (comment == null || comment.UserId != userId) 
                return null;

            comment.Content = request.Content.Trim();

            await _commentRepository.UpdateAsync(comment);

            return new { 
                message = "Comment updated successfully.", 
                comment.Id, 
                comment.PostId, 
                comment.UserId, 
                comment.Content, 
                comment.CreatedAt 
            };
        }

        public async Task<object?> GetCommentAsync(int id)
        {
            var comment = await _commentRepository.FindCommentByIdAsync(id);

            var name = await _userRepository.FindUserAsync(comment!.UserId);

            if (comment is null)
                return null;

            return new
            {
                comment.Id,
                comment.PostId,
                comment.UserId,
                name!.DisplayName,
                comment.Content,
                comment.CreatedAt
            };
        }
    }
}
