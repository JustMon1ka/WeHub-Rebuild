using Microsoft.EntityFrameworkCore;
using UserAuthService.Models;

namespace UserAuthService.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");

            entity.Property(e => e.UserId).HasColumnName("USER_ID");
            entity.Property(e => e.Username).HasColumnName("USERNAME");
            entity.Property(e => e.PasswordHash).HasColumnName("PASSWORD_HASH");
            entity.Property(e => e.Email).HasColumnName("EMAIL");
            entity.Property(e => e.Phone).HasColumnName("PHONE");
            entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");

            // ✅ 正确地映射 bool 到 Oracle 的 number(1)
            entity.Property(e => e.Status)
                .HasColumnName("STATUS")
                .HasConversion<int>();
        });
    }
}