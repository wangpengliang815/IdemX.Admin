namespace Core.Model.System;

public class SysOrgReq
{
    /// <summary>
    /// 上级 id，null 为顶级
    /// </summary>
    [Range(1, long.MaxValue)]
    public long? ParentId { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    /// <summary>
    /// 同级排序
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Sort { get; set; }
}
