using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteractHub.Api.Entities
{
    public class Hashtag
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public int UsageCount { get; set; } = 0;
    }
}
