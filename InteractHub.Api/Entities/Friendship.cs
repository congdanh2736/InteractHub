using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteractHub.Api.Entities
{
    public class Friendship
    {
        [Key]
        public int Id { get; set; }

        public string RequesterId { get; set; } = string.Empty;
        [ForeignKey("RequesterId")]
        public ApplicationUser? Requester { get; set; }

        public string ReceiverId { get; set; } = string.Empty;
        [ForeignKey("ReceiverId")]
        public ApplicationUser? Receiver { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
