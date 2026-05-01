using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteractHub.Api.Entities
{
    public class PostReport
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post? Post { get; set; }

        public string ReporterId { get; set; } = string.Empty; // nguoi bao cao
        [ForeignKey("ReporterId")]
        public ApplicationUser? Reporter { get; set; }

        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // pending, resolved, dismissed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
