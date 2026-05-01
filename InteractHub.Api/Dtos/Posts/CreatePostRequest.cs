namespace InteractHub.Api.Dtos.Posts
{
    public class CreatePostRequest
    {
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }


        //public string UserId { get; set; } = string.Empty;
        // không cần lấy userId vì đã lấy token 
    }
}
