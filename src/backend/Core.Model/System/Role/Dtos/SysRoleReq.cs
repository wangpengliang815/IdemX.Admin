namespace Core.Model.System;

public class SysRoleReq
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string RoleName { get; set; }

    /// <summary>
    /// 角色编码
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string RoleCode { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(255)]
    public string Memo { get; set; }
}
