using MediaService.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    public DbSet<Media> Medias => Set<Media>();
}