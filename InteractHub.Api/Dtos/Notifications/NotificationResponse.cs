namespace InteractHub.Api.Dtos.Notifications
{
    public class NotificationResponse
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
