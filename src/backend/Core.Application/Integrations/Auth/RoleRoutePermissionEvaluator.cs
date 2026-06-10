using System.Text.RegularExpressions;

namespace Core.Application;

public interface IPermissionEvaluator
{
    Task EnsurePermissionsLoadedAsync();

    bool HasRouteAccess(string requestPath, ClaimsPrincipal user, string claimType);

    bool IsTokenValid(ClaimsPrincipal user);
}

/// <summary>
/// 角色-路由权限：从库加载内存表，并按请求路径与用户角色校验访问
/// </summary>
public class RoleRoutePermissionEvaluator(
    IBaseRepo<SysRoleMenu> sysRoleMenuRepo,
    IBaseRepo<SysRole> roleRepo,
    IBaseRepo<SysMenu> menuRepo,
    RoleRoutePermissionStore roleRoutePermissionStore) : IPermissionEvaluator
{
    /// <summary>
    /// 首次访问时从数据库加载角色-菜单权限映射
    /// </summary>
    public async Task EnsurePermissionsLoadedAsync()
    {
        lock (roleRoutePermissionStore.PermissionSyncRoot)
        {
            if (roleRoutePermissionStore.Permissions.Count != 0)
                return;
        }

        // 数据不可能特别大，可接受全量加载
        var roleMenus = await sysRoleMenuRepo.GetListAsync();
        var roles = await roleRepo.GetListAsync();
        var menus = await menuRepo.GetListAsync();

        var roleDict = roles.ToDictionary(r => r.Id);
        var menuDict = menus.ToDictionary(m => m.Id);

        List<PermissionItem> list;

        if (Permissions.IsUseIds4)
        {
            list = [.. (from rm in roleMenus
                    let role = roleDict.TryGetValue(rm.RoleId, out SysRole value) ? value : null
                    let menu = menuDict.TryGetValue(rm.MenuId, out SysMenu value) ? value : null
                    where role is not null && menu is not null
                    orderby rm.Id
                    select new PermissionItem
                    {
                        Url = menu.Component,
                        RouteUrl = menu.Path,
                        Authority = menu.Authority,
                        Role = role.Id.ObjectToString(),
                    })];
        }
        else
        {
            list = [.. (from rm in roleMenus
                    let role = roleDict.TryGetValue(rm.RoleId, out SysRole value) ? value : null
                    let menu = menuDict.TryGetValue(rm.MenuId, out SysMenu value) ? value : null
                    where role is not null && menu is not null
                    orderby rm.Id
                    select new PermissionItem
                    {
                        Url = menu.Component,
                        RouteUrl = menu.Path,
                        Authority = menu.Authority,
                        Role = role.RoleCode,
                    })];
        }

        lock (roleRoutePermissionStore.PermissionSyncRoot)
        {
            if (roleRoutePermissionStore.Permissions.Count == 0)
                roleRoutePermissionStore.Permissions = list;
        }
    }

    /// <summary>
    /// 判断当前用户是否拥有访问当前路由所需的角色权限
    /// </summary>
    public bool HasRouteAccess(string requestPath, ClaimsPrincipal user, string claimType)
    {
        var currentUserRoles = GetCurrentUserRoles(user, claimType);
        if (currentUserRoles.Count <= 0)
            return false;

        var permissionRoles = roleRoutePermissionStore.Permissions.Where(w => currentUserRoles.Contains(w.Role));

        foreach (var item in permissionRoles)
        {
            if (string.IsNullOrEmpty(item.Url))
                continue;

            try
            {
                if (Regex.IsMatch(requestPath.ObjectToString(), item.Url, RegexOptions.IgnoreCase))
                    return true;
            }
            catch (Exception)
            {
            }
        }

        return false;
    }

    /// <summary>
    /// 校验当前用户 Token 是否过期
    /// </summary>
    public bool IsTokenValid(ClaimsPrincipal user)
    {
        if (Permissions.IsUseIds4)
        {
            var exp = user.Claims.SingleOrDefault(s => s.Type == "exp")?.Value;
            if (exp is null)
                return false;

            return DateHelper.StampToDateTime(exp) >= DateTime.Now;
        }

        var expiration = user.Claims.SingleOrDefault(s => s.Type == ClaimTypes.Expiration)?.Value;
        if (expiration is null)
            return false;

        return DateTime.Parse(expiration) >= DateTime.Now;
    }

    static List<string> GetCurrentUserRoles(ClaimsPrincipal user, string claimType)
    {
        if (Permissions.IsUseIds4)
        {
            return [.. (from item in user.Claims
                    where item.Type == "role"
                    select item.Value)];
        }

        return [.. (from item in user.Claims
                where item.Type == claimType
                select item.Value)];
    }
}
