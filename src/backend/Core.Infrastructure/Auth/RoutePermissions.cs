using Microsoft.AspNetCore.Authorization;

namespace Core.Infrastructure.Auth;

/// <summary>
/// 角色-路由权限项（内存权限表中的一行）
/// </summary>
public class PermissionItem
{
    public virtual string Role { get; set; }

    public virtual string Url { get; set; }

    public virtual string Authority { get; set; }

    public virtual string RouteUrl { get; set; }
}

/// <summary>
/// 角色-路由内存权限表失效（变更 sys_role_menu 后调用）
/// </summary>
public interface IPermissionCacheInvalidator
{
    void InvalidatePermissionCache();
}

/// <summary>
/// 角色-路由内存权限表（进程内缓存，变更菜单后需 Invalidate）
/// </summary>
public class RoleRoutePermissionStore : IPermissionCacheInvalidator
{
    readonly object permissionLock = new();

    public List<PermissionItem> Permissions { get; set; } = [];

    public object PermissionSyncRoot => permissionLock;

    /// <summary>
    /// 清空内存权限，下次鉴权时从库重新加载
    /// </summary>
    public void InvalidatePermissionCache()
    {
        lock (permissionLock)
        {
            Permissions = [];
        }
    }
}

/// <summary>
/// 基于路由的权限策略标记（实现 IAuthorizationRequirement，供 PermissionHandler 挂载）
/// </summary>
public class PermissionRequirement(string claimType) : IAuthorizationRequirement
{
    /// <summary>
    /// 角色 Claim 类型，非 IdentityServer 时通常与 ClaimTypes.Role 一致
    /// </summary>
    public string ClaimType { get; set; } = claimType;
}

/// <summary>
/// 权限配置（策略名、是否启用 IDS4）
/// </summary>
public static class Permissions
{
    public const string Name = "Permission";

    /// <summary>
    /// 是否启用 IDS4 权限方案（true：IDS4，false：JWT）
    /// </summary>
    public static bool IsUseIds4 = false;
}
