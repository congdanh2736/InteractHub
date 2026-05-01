namespace InteractHub.Api.Dtos.Friends
{
    public class SendFriendRequest
    {
        public string ReceiverId { get; set; } = string.Empty;
    }

    public class RespondFriendRequest
    {
        public string RequesterId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Accepted or Declined
    }

    public class FriendResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
