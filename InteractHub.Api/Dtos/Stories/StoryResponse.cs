namespace InteractHub.Api.Dtos.Stories
{
    public class StoryResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
