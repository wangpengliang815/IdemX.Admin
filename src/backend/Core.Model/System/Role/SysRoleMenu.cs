namespace Core.Model.System;

/// <summary>
/// 角色菜单关联
/// </summary>
[SugarTable("public.sys_role_menu")]
public class SysRoleMenu : BaseEntity
{
    /// <summary>
    /// 角色主键
    /// </summary>
    [Required]
    public long RoleId { get; set; }

    /// <summary>
    /// 菜单主键
    /// </summary>
    [Required]
    public long MenuId { get; set; }

    /// <summary>
    /// 关联菜单，仅查询填充，不入库
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public SysMenu Menu { get; set; }

    /// <summary>
    /// 关联角色，仅查询填充，不入库
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public SysRole Role { get; set; }
}
