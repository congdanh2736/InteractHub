using InteractHub.Api.Dtos.Notifications;

namespace InteractHub.Api.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
    }
}
