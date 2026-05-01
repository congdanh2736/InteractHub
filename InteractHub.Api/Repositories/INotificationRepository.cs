using InteractHub.Api.Entities;

namespace InteractHub.Api.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> AddNotificationAsync(Notification notification);
    }
}
