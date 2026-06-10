using System.Text.Json.Nodes;

namespace Core.Application;

/// <summary>
/// 运维冷启动：省市区、最小系统菜单
/// </summary>
public class InitService(
    IBaseRepo<SysArea> sysAreaRepo,
    IBaseRepo<SysUser> userRepo,
    IBaseRepo<SysRole> roleRepo,
    IBaseRepo<SysUserRole> userRoleRepo,
    IBaseRepo<SysMenu> menuRepo,
    IBaseRepo<SysRoleMenu> roleMenuRepo,
    IOptions<InitConfigOptions> initConfigOptions,
    IHttpContextUser contextUser,
    IUnitOfWork unitOfWork) : IInitService
{
    readonly InitConfigOptions initConfig = initConfigOptions.Value;

    /// <summary>
    /// 从 china_areas_data.json 全量重建省市区
    /// </summary>
    public async Task<CustomApiResponse<object>> InitAreasAsync(string contentRootPath)
    {
        if (!contextUser.IsAdmin)
            return CustomApiResponse<object>.Fail(GlobalConstVars.ViewPermissionDenied);

        var jsonFilePath = Path.Combine(contentRootPath, "Utilities", "china_areas_data.json");
        if (!File.Exists(jsonFilePath))
            return CustomApiResponse<object>.Fail("china_areas数据文件不存在");

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var rootData = JsonNode.Parse(json)?.AsArray();
        if (rootData == null || rootData.Count == 0)
            return CustomApiResponse<object>.Fail("省市区数据为空");

        var areasToAdd = new List<SysArea>();

        void ProcessJsonNode(JsonNode node, string parentCode, int level)
        {
            var code = node["code"]?.GetValue<string>()?.Trim() ?? "";
            var name = node["name"]?.GetValue<string>()?.Trim() ?? "";

            areasToAdd.Add(new SysArea
            {
                Code = code,
                Name = name,
                ParentCode = parentCode,
                Level = level,
            });

            var children = node["children"]?.AsArray();
            if (children != null && children.Count > 0)
            {
                foreach (var child in children)
                    ProcessJsonNode(child!, code, level + 1);
            }
        }

        foreach (var province in rootData)
            ProcessJsonNode(province!, "0", 1);

        const int batchSize = 2000;
        var total = areasToAdd.Count;

        await unitOfWork.BeginTranAsync();
        try
        {
            await sysAreaRepo.DeleteAsync(p => p.Id > 0);

            for (int i = 0; i < total; i += batchSize)
            {
                var batch = areasToAdd.Skip(i).Take(batchSize).ToList();
                await sysAreaRepo.InsertRangeAsync(batch);
            }

            await unitOfWork.CommitTranAsync();
        }
        catch
        {
            await unitOfWork.RollbackTranAsync();
            throw;
        }

        return CustomApiResponse<object>.Ok(
            "省市区数据初始化成功",
            new { TotalCount = total, InsertedCount = total },
            0);
    }

    /// <summary>
    /// 默认管理员、admin 角色与最小系统菜单（幂等，可重复执行）
    /// </summary>
    public async Task<CustomApiResponse<object>> InitProjectAsync()
    {
        if (!contextUser.IsAdmin)
            return CustomApiResponse<object>.Fail(GlobalConstVars.ViewPermissionDenied);

        var adminConfig = GetAdminConfig();
        if (adminConfig is null)
            return CustomApiResponse<object>.Fail("管理员基础信息配置不完整");

        await unitOfWork.BeginTranAsync();
        try
        {
            var adminRole = await roleRepo.GetFirstAsync(p => p.RoleCode == adminConfig.AdminRole.RoleCode);
            if (adminRole is null)
            {
                adminRole = new SysRole
                {
                    RoleName = adminConfig.AdminRole.RoleName,
                    RoleCode = adminConfig.AdminRole.RoleCode,
                    CreateTime = DateTime.Now
                };

                var roleId = await roleRepo.InsertAsync(adminRole);
                adminRole.Id = roleId;
            }

            var adminUser = await userRepo.GetFirstAsync(p => p.UserName == adminConfig.AdminUser.UserName);
            if (adminUser is null)
            {
                var now = DateTime.Now;
                adminUser = new SysUser
                {
                    UserName = adminConfig.AdminUser.UserName,
                    Password = PasswordHelper.Hash(adminConfig.AdminUser.Password),
                    RealName = adminConfig.AdminUser.RealName,
                    Sex = UserSexType.男,
                    Phone = adminConfig.AdminUser.Phone,
                    Email = adminConfig.AdminUser.Email ?? "",
                    UserType = UserType.内部用户,
                    Status = UserStatus.正常,
                    CreateTime = now
                };

                var userId = await userRepo.InsertAsync(adminUser);
                adminUser.Id = userId;
            }

            var adminUserRole = await userRoleRepo.GetFirstAsync(p =>
                p.UserId == adminUser.Id && p.RoleId == adminRole.Id);
            if (adminUserRole is null)
            {
                await userRoleRepo.InsertAsync(new SysUserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id,
                    CreateTime = DateTime.Now
                });
            }

            var systemId = await EnsureMenuAsync(
                name: "system",
                path: "/system",
                component: "/system/index",
                title: "后台管理",
                icon: "mdi:cog",
                parentId: null,
                sort: 999);

            var systemMenuId = await EnsureMenuAsync(
                name: "system_menu",
                path: "/system/menu",
                component: "/system/menu/index",
                title: "菜单管理",
                icon: "mdi:menu",
                parentId: systemId,
                sort: 1);

            var systemRoleMenuId = await EnsureMenuAsync(
                name: "system_role",
                path: "/system/role",
                component: "/system/role/index",
                title: "角色管理",
                icon: "mdi:filter-cog",
                parentId: systemId,
                sort: 2);

            await EnsureRoleMenuAsync(adminRole.Id, systemId);
            await EnsureRoleMenuAsync(adminRole.Id, systemMenuId);
            await EnsureRoleMenuAsync(adminRole.Id, systemRoleMenuId);

            await unitOfWork.CommitTranAsync();
        }
        catch
        {
            await unitOfWork.RollbackTranAsync();
            throw;
        }

        return CustomApiResponse<object>.Ok("项目种子数据初始化完成", null, 0);
    }

    InitConfigOptions GetAdminConfig()
    {
        var user = initConfig.AdminUser;
        var role = initConfig.AdminRole;

        if (string.IsNullOrWhiteSpace(user.UserName)
            || string.IsNullOrWhiteSpace(user.Password)
            || string.IsNullOrWhiteSpace(user.RealName)
            || string.IsNullOrWhiteSpace(user.Phone)
            || string.IsNullOrWhiteSpace(role.RoleName)
            || string.IsNullOrWhiteSpace(role.RoleCode))
            return null;

        return initConfig;
    }

    async Task<long> EnsureMenuAsync(
        string name,
        string path,
        string component,
        string title,
        string icon,
        long? parentId,
        int sort,
        int menuType = 0)
    {
        var menu = await menuRepo.GetFirstAsync(p =>
            p.ParentId == parentId && (p.Name == name || p.Path == path));
        if (menu is null)
        {
            menu = new SysMenu
            {
                ParentId = parentId,
                Name = name,
                Path = path,
                Component = component,
                Redirect = null,
                Title = title,
                Icon = icon,
                Sort = sort,
                Authority = null,
                Roles = null,
                AffixTab = false,
                IsExternal = false,
                ExternalUrl = null,
                IframeUrl = null,
                KeepAlive = true,
                MenuType = menuType,
                Badge = null,
                BadgeType = null,
                BadgeVariants = null,
                ActiveMenu = null,
                BreadcrumbParentIcon = null,
                Link = null,
                Status = 1,
                CreateTime = DateTime.Now
            };

            var id = await menuRepo.InsertAsync(menu);
            return id;
        }

        return menu.Id;
    }

    async Task EnsureRoleMenuAsync(long roleId, long menuId)
    {
        var exists = await roleMenuRepo.GetFirstAsync(p =>
            p.RoleId == roleId && p.MenuId == menuId);
        if (exists is not null)
            return;

        await roleMenuRepo.InsertAsync(new SysRoleMenu
        {
            RoleId = roleId,
            MenuId = menuId,
            CreateTime = DateTime.Now
        });
    }
}
