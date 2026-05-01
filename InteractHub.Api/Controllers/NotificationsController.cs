using InteractHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InteractHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotification()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead([FromRoute] int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized();
            }

            var success = await _notificationService.MarkAsReadAsync(id, userId);
            if (!success)
                return NotFound("Notification not found or unauthorized.");

            return Ok(new
            {
                message = "Notification marked as read."
            });
        }
    }
}
