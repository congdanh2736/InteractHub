using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Notifications;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification is null)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
