namespace SiemensCommunicator.Core.Models;

/// <summary>
/// PLC 标签实体
/// </summary>
[Table("PlcTags")]
[Comment("PLC标签表")]
public class PlcTag
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("Id")]
    [Comment("主键ID")]
    public long Id { get; set; }

    /// <summary>
    /// 标签名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("Name")]
    [Comment("标签名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 标签颜色（十六进制颜色代码）
    /// </summary>
    [Required]
    [MaxLength(7)]
    [Column("Color")]
    [Comment("标签颜色")]
    public string Color { get; set; } = "#000000";
}
