using Microsoft.EntityFrameworkCore;
using NoticeService.Models;

namespace NoticeService.Data
{
    public class NoticeDbContext : DbContext
    {
        public NoticeDbContext(DbContextOptions<NoticeDbContext> options) : base(options) { }

        public DbSet<Reply> Replies { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Repost> Reposts { get; set; }
        public DbSet<Mention> Mentions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reply>()
                .HasKey(r => r.ReplyId);
            modelBuilder.Entity<Reply>()
                .HasIndex(r => new { r.TargetUserId, r.IsDeleted, r.CreatedAt });

            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.UserId, l.TargetType, l.TargetId });
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.TargetType, l.TargetId, l.CreatedAt });

            modelBuilder.Entity<Repost>()
                .HasKey(rp => rp.RepostId);
            modelBuilder.Entity<Repost>()
                .HasIndex(rp => new { rp.TargetUserId, rp.CreatedAt });

            modelBuilder.Entity<Mention>()
                .HasKey(m => new { m.UserId, m.TargetType, m.TargetId });
            modelBuilder.Entity<Mention>()
                .HasIndex(m => new { m.TargetUserId, m.CreatedAt });
        }
    }
}