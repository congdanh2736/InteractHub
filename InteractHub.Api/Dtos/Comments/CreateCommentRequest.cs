namespace InteractHub.Api.Dtos.Comments
{
    public class CreateCommentRequest
    {
        public int PostId { get; set; }
        //public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
