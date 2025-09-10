using Microsoft.EntityFrameworkCore;
using FollowService.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FollowService.Data
{
    public class FollowDbContext : DbContext
    {
        public FollowDbContext(DbContextOptions<FollowDbContext> options)
            : base(options)
        {
        }
        public DbSet<Follow> Follows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FolloweeId });
        }
    }
}