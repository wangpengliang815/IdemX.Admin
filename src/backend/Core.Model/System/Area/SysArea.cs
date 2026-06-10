namespace Core.Model.System;

/// <summary>
/// 省市区数据
/// </summary>
[SugarTable("public.sys_area")]
public class SysArea : BaseEntity
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
    /// 父级编码
    /// </summary>
    public string ParentCode { get; set; }

    /// <summary>
    /// 区域层级
    /// 省 ：1
    /// 市：2
    /// 区：3
    /// 街道：4
    /// </summary>
    public int Level { get; set; }
}
