using InteractHub.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts => Set<Post>(); // Post table
        public DbSet<Like> Likes => Set<Like>(); // Like table
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<PostReport> PostReports { get; set; }

        // cau hinh cac moi quan he phuc tap hon
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Comment>().ToTable("Comment");
            builder.Entity<Like>().ToTable("Like");

            // Cau hinh cho Comment
            builder.Entity<Comment>()
                .HasOne(c => c.Post) // lay property Post trong class Comment
                .WithMany(p => p.Comments) // lay property collection trong class Post
                .HasForeignKey(c => c.PostId) // chi ro cot khoa ngoai trong bang Comment là PostId
                .OnDelete(DeleteBehavior.NoAction); 

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            //---------------------

            // Cau hinh cho Like
            builder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            //---------------------

            //Cau hinh cho Friendship
            builder.Entity<Friendship>()
                .HasOne(f => f.Requester)
                .WithMany()
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friendship>()
                .HasOne(f => f.Receiver)
                .WithMany()
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            //---------------------

            //Cau hinh cho PostReport
            builder.Entity<PostReport>()
                .HasOne(pr => pr.Reporter)
                .WithMany()
                .HasForeignKey(pr => pr.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);
            //---------------------

            
            
        }
    }
}
