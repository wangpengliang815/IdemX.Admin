namespace Core.Model.System;

public class SysUserPageQueryReq : BasePageQueryReq
{
    /// <summary>
    /// 按登录名模糊筛，可空
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 按状态筛，可空
    /// </summary>
    public UserStatus? Status { get; set; }

    /// <summary>
    /// 按角色筛，可空
    /// </summary>
    public long? RoleId { get; set; }
}
