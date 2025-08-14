using Microsoft.EntityFrameworkCore;
using UserTagService.Models;

namespace UserTagService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<UserTag> UserTags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserTag>()
            .HasKey(ut => new { ut.UserId, ut.TagId }); // 联合主键
    }
}