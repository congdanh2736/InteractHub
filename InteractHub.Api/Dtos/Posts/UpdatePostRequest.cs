namespace InteractHub.Api.Dtos.Posts
{
    public class UpdatePostRequest
    {
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
