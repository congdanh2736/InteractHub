using Microsoft.AspNetCore.Identity;

namespace InteractHub.Api.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty; // ten nguoi dung
        public string? AvatarUrl { get; set; } // anh dai dien
        public string? Bio { get; set; }

        public ICollection<Post> Posts { get; set; } = new List<Post>(); // mot User co the co nhieu Post
        public ICollection<Comment> Comments { get; set; } = new List<Comment>(); // mot User co the co nhieu Comment
        public ICollection<Like> Likes { get; set; } = new List<Like>(); // co the co nhieu Like

    }
}
