using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Core.Models;

/// <summary>
/// PLC 连接配置实体
/// </summary>
[Table("PlcConnections")]
[Comment("PLC连接配置表")]
public class PlcConnection
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("Id")]
    [Comment("主键ID")]
    public long Id { get; set; }

    /// <summary>
    /// 连接名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("Name")]
    [Comment("连接名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// PLC CPU 类型
    /// </summary>
    [Required]
    [Column("CpuType")]
    [Comment("PLC CPU类型")]
    public CpuType CpuType { get; set; }

    /// <summary>
    /// PLC IP 地址
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("Ip")]
    [Comment("PLC IP地址")]
    public string Ip { get; set; } = string.Empty;

    /// <summary>
    /// 机架号
    /// </summary>
    [Required]
    [Column("Rack")]
    [Comment("机架号")]
    public int Rack { get; set; }

    /// <summary>
    /// 插槽号
    /// </summary>
    [Required]
    [Column("Slot")]
    [Comment("插槽号")]
    public int Slot { get; set; }

    /// <summary>
    /// 缓存刷新间隔（毫秒）
    /// </summary>
    [Required]
    [Column("CacheRefreshInterval")]
    [Comment("缓存刷新间隔（毫秒）")]
    public int CacheRefreshInterval { get; set; } = 1000;

    /// <summary>
    /// 是否激活
    /// </summary>
    [Required]
    [Column("IsActive")]
    [Comment("是否激活")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    [Column("CreatedAt")]
    [Comment("创建时间")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    [Required]
    [Column("UpdatedAt")]
    [Comment("更新时间")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 数据点集合
    /// </summary>
    public ICollection<PlcDataPoint> DataPoints { get; set; } = new List<PlcDataPoint>();
}
