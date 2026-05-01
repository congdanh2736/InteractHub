using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteractHub.Api.Entities
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        //Fk toi Post
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; } = null!;

        //Fk toi User
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
