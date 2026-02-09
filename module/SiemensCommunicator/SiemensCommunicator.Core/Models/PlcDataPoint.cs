using PlcDataType = SiemensCommunicator.Core.Models.Enums.DataType;

namespace SiemensCommunicator.Core.Models;

/// <summary>
/// PLC 数据点配置实体
/// </summary>
[Table("PlcDataPoints")]
[Comment("PLC数据点配置表")]
public class PlcDataPoint
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("Id")]
    [Comment("主键ID")]
    public long Id { get; set; }

    /// <summary>
    /// 关联的连接ID
    /// </summary>
    [Required]
    [Column("ConnectionId")]
    [Comment("关联的连接ID")]
    public long ConnectionId { get; set; }

    /// <summary>
    /// 分组ID
    /// </summary>
    [Column("GroupId")]
    [Comment("分组ID")]
    public long? GroupId { get; set; }

    /// <summary>
    /// 数据点名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("Name")]
    [Comment("数据点名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数据点描述
    /// </summary>
    [MaxLength(500)]
    [Column("Description")]
    [Comment("数据点描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    [Required]
    [Column("DataType")]
    [Comment("数据类型")]
    public PlcDataType DataType { get; set; }

    /// <summary>
    /// 数据块编号
    /// </summary>
    [Required]
    [Column("DbNumber")]
    [Comment("数据块编号")]
    public int DbNumber { get; set; }

    /// <summary>
    /// 起始字节
    /// </summary>
    [Required]
    [Column("StartByte")]
    [Comment("起始字节")]
    public int StartByte { get; set; }

    /// <summary>
    /// 位偏移
    /// </summary>
    [Required]
    [Column("BitOffset")]
    [Comment("位偏移")]
    public int BitOffset { get; set; }

    /// <summary>
    /// 变量类型
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("VarType")]
    [Comment("变量类型")]
    public string VarType { get; set; } = string.Empty;

    /// <summary>
    /// 数据数量
    /// </summary>
    [Required]
    [Column("DataCount")]
    [Comment("数据数量")]
    public int DataCount { get; set; } = 1;

    /// <summary>
    /// 是否启用缓存
    /// </summary>
    [Required]
    [Column("EnableCache")]
    [Comment("是否启用缓存")]
    public bool EnableCache { get; set; } = true;

    /// <summary>
    /// 单位
    /// </summary>
    [MaxLength(20)]
    [Column("Unit")]
    [Comment("单位")]
    public string? Unit { get; set; }

    /// <summary>
    /// 转换公式
    /// </summary>
    [MaxLength(200)]
    [Column("ConversionFormula")]
    [Comment("转换公式")]
    public string? ConversionFormula { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    [Column("MinValue")]
    [Comment("最小值")]
    public decimal? MinValue { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    [Column("MaxValue")]
    [Comment("最大值")]
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// 标签集合（JSON 格式存储）
    /// </summary>
    [Column("Tags")]
    [Comment("标签集合（JSON格式）")]
    public string? Tags { get; set; }

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
    /// 关联的PLC连接
    /// </summary>
    [ForeignKey("ConnectionId")]
    public PlcConnection? Connection { get; set; }

    /// <summary>
    /// 关联的分组
    /// </summary>
    [ForeignKey("GroupId")]
    public PlcGroup? Group { get; set; }
}
