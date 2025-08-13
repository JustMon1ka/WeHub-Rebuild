using Microsoft.EntityFrameworkCore;
using ReportService.Models;

namespace ReportService.Data
{
    public class ReportDbContext : DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options)
            : base(options)
        {
        }

        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>()
                .HasKey(r => r.ReportId);

            modelBuilder.Entity<Report>()
                .Property(r => r.TargetType)
                .HasConversion<string>(); // ENUM 转换为字符串
        }
    }
}