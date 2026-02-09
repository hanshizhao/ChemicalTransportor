namespace SiemensCommunicator.Core.Models;

/// <summary>
/// PLC 分组实体
/// </summary>
[Table("PlcGroups")]
[Comment("PLC分组表")]
public class PlcGroup
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("Id")]
    [Comment("主键ID")]
    public long Id { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("Name")]
    [Comment("分组名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分组描述
    /// </summary>
    [MaxLength(500)]
    [Column("Description")]
    [Comment("分组描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    [Required]
    [Column("SortOrder")]
    [Comment("排序顺序")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 数据点集合
    /// </summary>
    public ICollection<PlcDataPoint> DataPoints { get; set; } = new List<PlcDataPoint>();
}
