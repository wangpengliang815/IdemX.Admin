namespace Core.Model.System;

public class SysUserPageQueryReq : BasePageQueryReq
{
    /// <summary>
    /// 按登录名模糊筛，可空
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 按实名模糊筛，可空
    /// </summary>
    public string RealName { get; set; }

    /// <summary>
    /// 按状态筛，可空
    /// </summary>
    public UserStatus? Status { get; set; }

    /// <summary>
    /// 按用户类型筛，可空
    /// </summary>
    public UserType? UserType { get; set; }

    /// <summary>
    /// 按角色筛，可空
    /// </summary>
    public long? RoleId { get; set; }
}
