namespace Core.Infrastructure.Options;

/// <summary>
/// 系统初始化种子数据（appsettings 节名：InitConfig）
/// </summary>
public class InitConfigOptions
{
    public AdminUserOptions AdminUser { get; set; } = new();

    public AdminRoleOptions AdminRole { get; set; } = new();
}

/// <summary>
/// 内置管理员账号
/// </summary>
public class AdminUserOptions
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string RealName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// 内置管理员角色
/// </summary>
public class AdminRoleOptions
{
    public string RoleName { get; set; } = string.Empty;

    public string RoleCode { get; set; } = string.Empty;
}
