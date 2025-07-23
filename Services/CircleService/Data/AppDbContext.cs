using Microsoft.EntityFrameworkCore;
using CircleService.Models;

namespace CircleService.Data;

/// <summary>
/// 应用程序的数据库上下文，负责与数据库进行交互。
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 圈子表
    /// </summary>
    public DbSet<Circle> Circles { get; set; }
    
    /// <summary>
    /// 圈子成员表
    /// </summary>
    public DbSet<CircleMember> CircleMembers { get; set; }
    
    /// <summary>
    /// 活动表
    /// </summary>
    public DbSet<Activity> Activities { get; set; }
    
    /// <summary>
    /// 通知表
    /// </summary>
    public DbSet<Notification> Notifications { get; set; }
    
    /// <summary>
    /// 用户参与活动记录表
    /// </summary>
    public DbSet<ActivityParticipant> ActivityParticipants { get; set; }

    /// <summary>
    /// 在模型创建时进行额外配置
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 为具有复合主键的表进行配置
        modelBuilder.Entity<CircleMember>()
            .HasKey(cm => new { cm.CircleId, cm.UserId });

        modelBuilder.Entity<ActivityParticipant>()
            .HasKey(ap => new { ap.ActivityId, ap.UserId });
    }
} 