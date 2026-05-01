using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Likes;
using InteractHub.Api.Entities;
using Microsoft.EntityFrameworkCore;

// Thư viện Hubs và signalR để gửi thông báo cho người dùng rằng có ai đó đã like
using InteractHub.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using InteractHub.Api.Repositories;

namespace InteractHub.Api.Services
{
    public class LikeService : ILikeService
    {
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LikeService(IPostRepository postRepository, ILikeRepository likeRepository, IHubContext<NotificationHub> hubContext)
        {
            _postRepository = postRepository;
            _hubContext = hubContext;
            _likeRepository = likeRepository;
        }

        public async Task<object?> ToggleLikeAsync(ToggleLikeRequest request, string userId)
        {
            var postExists = await _postRepository.GetByIdAsync(request.PostId);
            if (postExists is null)
                return null;

            var existingLike = await _likeRepository.GetLikeByIdAsync(request.PostId, userId);
            if (existingLike is not null)
            {
                await _likeRepository.RemoveAsync(existingLike);

                // Gửi cho hub thông báo rằng có người đã unlike
                await _hubContext.Clients.All.SendAsync("ReceiveLikeUpdate", request.PostId);
                //~~~~~~~~

                return new
                {
                    message = "Unliked",
                    isLiked = false,
                };
            }

            var like = new Like
            {
                PostId = request.PostId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            await _likeRepository.AddAsync(like);

            // Gửi cho hub thông báo rằng có người đã like
            await _hubContext.Clients.All.SendAsync("ReceiveLikeUpdate", request.PostId);
            //~~~~~~~~

            return new
            {
                message = "Liked",
                isLiked = true,
            };
        }
    }
}
