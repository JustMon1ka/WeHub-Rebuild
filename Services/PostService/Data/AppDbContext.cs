using Microsoft.EntityFrameworkCore;
using Models;

namespace PostService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Circle> Circles => Set<Circle>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<PostTag>()
            .HasKey(pt => new { pt.PostId, pt.TagId });

        modelBuilder.Entity<PostTag>()
            .HasOne(pt => pt.Post)
            .WithMany(p => p.PostTags)
            .HasForeignKey(pt => pt.PostId);

        modelBuilder.Entity<PostTag>()
            .HasOne(pt => pt.Tag)
            .WithMany(t => t.PostTags)
            .HasForeignKey(pt => pt.TagId);
        
        // --- 显式映射 Post 与 User / Circle ---
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("POST");
            entity.HasKey(e => e.PostId);
            entity.Property(e => e.PostId).HasColumnName("POST_ID");

            entity.Property(e => e.UserId).HasColumnName("USER_ID").IsRequired(false);
            entity.Property(e => e.CircleId).HasColumnName("CIRCLE_ID");

            entity.Property(e => e.Title).HasColumnName("TITLE").IsRequired(false);
            entity.Property(e => e.Content).HasColumnName("CONTENT").IsRequired(false);
            entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired(false);
            entity.Property(e => e.IsDeleted).HasColumnName("IS_DELETED");
            entity.Property(e => e.IsHidden).HasColumnName("IS_HIDDEN");
            entity.Property(e => e.Views).HasColumnName("VIEWS").IsRequired(false);
            entity.Property(e => e.Likes).HasColumnName("LIKES").IsRequired(false);
            entity.Property(e => e.Dislikes).HasColumnName("DISLIKES").IsRequired(false);
            // 其他列同理，若允许为 NULL 则 IsRequired(false)

            // 绑定关系 — 明确告诉 EF 外键使用 Post.UserId
            entity.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .HasPrincipalKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull); // 根据需求选择行为

            entity.HasOne(p => p.Circle)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CircleId)
                .HasPrincipalKey(c => c.CircleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // User 映射（确保用户名为可选）
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.UserId).HasColumnName("USER_ID");
            entity.Property(u => u.Username).HasColumnName("USERNAME").IsRequired(false);
        });

        // Circle & Tag 映射示例
        modelBuilder.Entity<Circle>(entity =>
        {
            entity.ToTable("CIRCLE");
            entity.HasKey(c => c.CircleId);
            entity.Property(c => c.CircleId).HasColumnName("CIRCLE_ID");
            entity.Property(c => c.Name).HasColumnName("NAME").IsRequired(false);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("TAG");
            entity.HasKey(t => t.TagId);
            entity.Property(t => t.TagId).HasColumnName("TAG_ID");
            entity.Property(t => t.TagName).HasColumnName("TAG_NAME").IsRequired(false);
        });
    }
}