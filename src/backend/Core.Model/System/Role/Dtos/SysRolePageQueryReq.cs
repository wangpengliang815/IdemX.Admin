namespace Core.Model.System;

public class SysRolePageQueryReq : BasePageQueryReq
{
    /// <summary>
    /// 按名称筛选，可空
    /// </summary>
    public string RoleName { get; set; }

    /// <summary>
    /// 按编码筛选，可空
    /// </summary>
    public string RoleCode { get; set; }
}
