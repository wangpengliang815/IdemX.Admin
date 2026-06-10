namespace Core.Model.System;

/// <summary>
/// 省市区联动列表项
/// </summary>
public class SysAreaItemResp
{
    /// <summary>
    /// 区域编号
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 父级编码
    /// </summary>
    public string ParentCode { get; set; }
}
