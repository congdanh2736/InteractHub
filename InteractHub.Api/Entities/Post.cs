using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteractHub.Api.Entities
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty; // noi dung bai viet
        public string? ImageUrl { get; set; } // hinh anh neu co
        public DateTime CreatedAt { get; set; } // thoi gian tao bai viet

        // Quan he voi User
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!; // EF se tu map bang UserId


        // cac quan he 1-n
        public ICollection<Comment> Comments { get; set; } = new List<Comment>(); // mot Post co the co nhieu Comment
        public ICollection<Like> Likes { get; set; } = new List<Like>(); // co the co nhieu Like
        public ICollection<PostReport> PostReports { get; set; } = new List<PostReport>();

    }
}
