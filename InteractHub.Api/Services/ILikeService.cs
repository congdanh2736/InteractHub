using InteractHub.Api.Dtos.Likes;

namespace InteractHub.Api.Services
{
    public interface ILikeService
    {
        Task<object?> ToggleLikeAsync(ToggleLikeRequest request, string userId);
    }
}
