namespace Core.Model.System;

public class SysRoleResp : BaseResp
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }

    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Memo { get; set; }
}
