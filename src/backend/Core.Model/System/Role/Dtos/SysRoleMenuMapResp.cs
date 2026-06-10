namespace Core.Model.System;

public class SysRoleMenuMapResp
{
    /// <summary>
    /// 菜单树，供前端 Tree 展示
    /// </summary>
    public List<SysRoleMenuMapTreeNodeResp> TreeData { get; set; } = [];

    /// <summary>
    /// 全选节点主键（叶子或子孙已全部勾选）
    /// </summary>
    public List<long> CheckedKeys { get; set; } = [];

    /// <summary>
    /// 半选节点主键（已分配但子孙未全选）
    /// </summary>
    public List<long> HalfCheckedKeys { get; set; } = [];
}

public class SysRoleMenuMapTreeNodeResp
{
    /// <summary>
    /// 节点 id，与菜单主键一致
    /// </summary>
    public long Key { get; set; }

    /// <summary>
    /// 父级 id，根为 null
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// 节点标题，一般为菜单 Title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 0 菜单 1 按钮
    /// </summary>
    public int MenuType { get; set; }

    /// <summary>
    /// 同级排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 子节点，无子为空数组
    /// </summary>
    public List<SysRoleMenuMapTreeNodeResp> Children { get; set; } = [];
}
