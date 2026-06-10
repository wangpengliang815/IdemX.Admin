namespace Core.Model.System;

/// <summary>
/// 用户与角色多对多关联
/// </summary>
[SugarTable("public.sys_user_role")]
public class SysUserRole : BaseEntity
{
    /// <summary>
    /// 用户主键
    /// </summary>
    [Required]
    public long UserId { get; set; }

    /// <summary>
    /// 角色主键
    /// </summary>
    [Required]
    public long RoleId { get; set; }
}
