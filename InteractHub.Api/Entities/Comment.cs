using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteractHub.Api.Entities
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty; // noi dung binh luan
        public DateTime CreatedAt { get; set; } // thoi diem tao comment

        // quan he voi Post
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; } = null!;

        // quan he voi User, nguoi binh luan
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
