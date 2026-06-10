namespace Core.Model.System;

public class SysOrgResp : BaseResp
{
    /// <summary>
    /// 上级 id，null 为根节点
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 同级排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否存在子级（懒加载树用）
    /// </summary>
    public bool HasChild { get; set; }
}
