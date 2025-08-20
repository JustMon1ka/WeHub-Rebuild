using Microsoft.EntityFrameworkCore;
using NoticeService.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NoticeService.Data
{
    public class NoticeDbContext : DbContext
    {
        public DbSet<Reply> Replies { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Repost> Reposts { get; set; }
        public DbSet<Mention> Mentions { get; set; }

        public NoticeDbContext(DbContextOptions<NoticeDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 配置复合主键等
            modelBuilder.Entity<Like>().HasKey(l => new { l.UserId, l.TargetId });
            modelBuilder.Entity<Mention>().HasKey(m => new { m.UserId, m.TargetId });
            // 添加外键约束等，根据之前表结构
        }
    }
}