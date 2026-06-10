namespace Core.Model.System;

/// <summary>
/// 省市区树节点（与级联 options：label / value / children 对齐）
/// </summary>
public class SysAreaTreeNodeResp
{
    /// <summary>
    /// 展示名称
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// 区域编码
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 子节点；区级或无下级时为 null
    /// </summary>
    public List<SysAreaTreeNodeResp> Children { get; set; }
}
