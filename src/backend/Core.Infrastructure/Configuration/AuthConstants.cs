namespace Core.Infrastructure.Configuration;

/// <summary>
/// 身份与权限相关常量（角色编码、Claim 类型等）
/// </summary>
public static class AuthConstants
{
    /// <summary>管理员角色编码（拥有此角色视为平台管理员，可看全部数据等）</summary>
    public const string AdminRoleCode = "admin";

    /// <summary>注册用户默认角色编码（与 seed.sql 一致，自助注册后自动绑定）</summary>
    public const string RegisteredUserRoleCode = "registered";

    /// <summary>平台管理员账号用户名（系统内唯一，用于平台方收益绑定等场景）</summary>
    public const string AdminUserName = "admin";

    /// <summary>JWT Claim：是否管理员，值为 "true"</summary>
    public const string ClaimIsAdmin = "is_admin";
}

/// <summary>
/// 操作人常量（用于标识系统操作）
/// </summary>
public static class OperatorConstants
{
    /// <summary>
    /// 系统操作
    /// </summary>
    public const long System = -1;

    /// <summary>
    /// 数据导入任务
    /// </summary>
    public const long ImportTask = -2;

    /// <summary>
    /// 同步任务
    /// </summary>
    public const long SyncJob = -3;

    /// <summary>
    /// 初始化数据
    /// </summary>
    public const long InitData = -100;
}
