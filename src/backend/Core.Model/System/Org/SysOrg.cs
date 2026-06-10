namespace Core.Model.System;

/// <summary>
/// 组织机构
/// </summary>
[SugarTable("public.sys_org")]
public class SysOrg : BaseEntity
{
    /// <summary>
    /// 上级Id，null 代表根节点
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public long? ParentId { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Sort { get; set; }
}