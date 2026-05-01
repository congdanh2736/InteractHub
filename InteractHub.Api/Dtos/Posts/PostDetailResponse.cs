using InteractHub.Api.Entities;

namespace InteractHub.Api.Dtos.Posts
{
    public class PostDetailResponse
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public bool IsLiked { get; set; }
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
    }
}
