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
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reply>()
                .HasKey(r => r.ReplyId);
            modelBuilder.Entity<Reply>()
                .HasIndex(r => new { r.TargetUserId, r.IsDeletedNumber, r.CreatedAt });

            // 配置Reply的IsDeleted字段类型转换
            modelBuilder.Entity<Reply>()
                .Property(r => r.IsDeletedNumber)
                .HasColumnName("IS_DELETED")
                .HasColumnType("NUMBER(22)");

            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.UserId, l.TargetType, l.TargetId });
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.TargetType, l.TargetId, l.LikeTime });

            // 配置Like实体的字段映射
            modelBuilder.Entity<Like>()
                .Property(l => l.LikeTime)
                .HasColumnName("LIKE_TIME")
                .HasColumnType("DATETIME");

            modelBuilder.Entity<Like>()
                .Property(l => l.TargetUserId)
                .HasColumnName("TARGET_USER_ID");

            modelBuilder.Entity<Repost>()
                .HasKey(rp => rp.RepostId);
            modelBuilder.Entity<Repost>()
                .HasIndex(rp => new { rp.TargetUserId, rp.CreatedAt });

            modelBuilder.Entity<Mention>()
                .HasKey(m => new { m.UserId, m.TargetType, m.TargetId });
            modelBuilder.Entity<Mention>()
                .HasIndex(m => new { m.TargetUserId, m.CreatedAt });

            modelBuilder.Entity<Comment>()
                .HasKey(c => c.CommentId);
            modelBuilder.Entity<Comment>()
                .HasIndex(c => new { c.TargetUserId, c.IsDeletedNumber, c.CreatedAt });

            // 配置Comment的IsDeleted字段类型转换
            modelBuilder.Entity<Comment>()
                .Property(c => c.IsDeletedNumber)
                .HasColumnName("IS_DELETED")
                .HasColumnType("NUMBER(22)");
        }
    }
}