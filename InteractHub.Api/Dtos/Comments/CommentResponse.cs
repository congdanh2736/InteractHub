namespace InteractHub.Api.Dtos.Comments
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
