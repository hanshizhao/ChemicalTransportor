using Microsoft.EntityFrameworkCore;

namespace SiemensCommunicator.EntityFramework.Core.DbContexts;

/// <summary>
/// SiemensCommunicator 数据库上下文
/// </summary>
public class SiemensCommunicatorDbContext : DbContext
{
    /// <summary>
    /// PLC 连接配置
    /// </summary>
    public DbSet<PlcConnection> PlcConnections { get; set; }

    /// <summary>
    /// PLC 数据点配置
    /// </summary>
    public DbSet<PlcDataPoint> PlcDataPoints { get; set; }

    /// <summary>
    /// PLC 分组
    /// </summary>
    public DbSet<PlcGroup> PlcGroups { get; set; }

    /// <summary>
    /// PLC 标签
    /// </summary>
    public DbSet<PlcTag> PlcTags { get; set; }

    public SiemensCommunicatorDbContext(DbContextOptions<SiemensCommunicatorDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PlcConnection 配置
        modelBuilder.Entity<PlcConnection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsActive);

            entity.HasMany(e => e.DataPoints)
                .WithOne(d => d.Connection)
                .HasForeignKey(d => d.ConnectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlcDataPoint 配置
        modelBuilder.Entity<PlcDataPoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConnectionId);
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.Name);

            entity.HasOne(d => d.Group)
                .WithMany(g => g.DataPoints)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PlcGroup 配置
        modelBuilder.Entity<PlcGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.SortOrder);
        });

        // PlcTag 配置
        modelBuilder.Entity<PlcTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
}
