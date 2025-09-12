using MessageService.Models;
using Microsoft.EntityFrameworkCore;

namespace MessageService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasKey(m => m.MessageId);

            modelBuilder.Entity<Message>()
                .Property(m => m.Content)
                .HasColumnType("TEXT");

            modelBuilder.Entity<Message>()
                .Property(m => m.SentAt)
                .HasColumnType("DATETIME");

            modelBuilder.Entity<Message>()
                .Property(m => m.IsRead)
                .HasColumnName("IS_READ")
                .HasColumnType("NUMBER(1)")
                .HasConversion<int>();
        }
    }
}