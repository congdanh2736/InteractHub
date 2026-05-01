using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteractHub.Api.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty; // nguoi nhan thong bao
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // 1. like, 2.comment, 3.friend request
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
