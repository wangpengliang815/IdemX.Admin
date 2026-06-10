namespace Core.Model.System;

/// <summary>
/// 系统角色
/// </summary>
[SugarTable("public.sys_role")]
public class SysRole : BaseEntity
{
    /// <summary>
    /// 角色显示名
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; }

    /// <summary>
    /// 角色编码，业务唯一标识
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RoleCode { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [MaxLength(255)]
    public string Memo { get; set; }
}
