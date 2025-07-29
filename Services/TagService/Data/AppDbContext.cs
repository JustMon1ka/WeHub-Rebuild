using Microsoft.EntityFrameworkCore;
using TagService.Models;

namespace TagService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    public DbSet<Tag> Tags => Set<Tag>();
}