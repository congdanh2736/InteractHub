using Microsoft.AspNetCore.Identity;

namespace InteractHub.Api.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty; 
        public string? AvatarUrl { get; set; } 
        public string? Bio { get; set; }

        public ICollection<Post> Posts { get; set; } = new List<Post>(); 
        public ICollection<Comment> Comments { get; set; } = new List<Comment>(); 
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
