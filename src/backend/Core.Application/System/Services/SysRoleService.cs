namespace Core.Application;

public class SysRoleService(IBaseRepo<SysRole> roleRepo
    , IBaseRepo<SysMenu> menuRepo
    , IBaseRepo<SysRoleMenu> roleMenuRepo
    , IBaseRepo<SysUserRole> userRoleRepo
    , IUnitOfWork unitOfWork
    , IPermissionCacheInvalidator permissionCacheInvalidator
    , IMapper mapper) : ISysRoleService
{
    /// <summary>
    /// 获取全部角色列表
    /// </summary>
    /// <returns></returns>
    public async Task<CustomApiResponse<List<SysRoleResp>>> GetListAsync()
    {
        var roles = await roleRepo.GetListAsync(null, p => p.CreateTime, OrderByType.Desc);
        return CustomApiResponse<List<SysRoleResp>>.Ok(GlobalConstVars.GetDataSuccess, mapper.Map<List<SysRoleResp>>(roles));
    }

    /// <summary>
    /// 分页查询角色列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<List<SysRoleResp>>> GetPageListAsync(SysRolePageQueryReq request)
    {
        var where = PredicateBuilder.True<SysRole>();

        if (!string.IsNullOrEmpty(request.RoleName))
            where = where.And(p => p.RoleName.Contains(request.RoleName));

        if (!string.IsNullOrEmpty(request.RoleCode))
            where = where.And(p => p.RoleCode.Contains(request.RoleCode));

        var list = await roleRepo.GetPageAsync(where, p => p.Id, OrderByType.Desc, request.Page, request.PageSize);

        return CustomApiResponse<List<SysRoleResp>>.Ok(GlobalConstVars.GetDataSuccess, mapper.Map<List<SysRoleResp>>(list), list.TotalCount);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> CreateAsync(SysRoleReq request)
    {
        if (await roleRepo.IsAnyAsync(r => r.RoleName == request.RoleName))
            return CustomApiResponse.Fail(GlobalConstVars.RoleNameDuplicateFailure);

        if (await roleRepo.IsAnyAsync(r => r.RoleCode == request.RoleCode))
            return CustomApiResponse.Fail(GlobalConstVars.RoleCodeDuplicateFailure);

        var entity = mapper.Map<SysRole>(request);
        var newId = await roleRepo.InsertAsync(entity);
        return newId > 0
            ? CustomApiResponse.Ok(GlobalConstVars.CreateSuccess)
            : CustomApiResponse.Fail(GlobalConstVars.CreateFailure);
    }

    /// <summary>
    /// 根据主键获取角色
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<SysRoleResp>> GetByIdAsync(long id)
    {
        var model = await roleRepo.GetByIdAsync(id);
        if (model is null)
            return CustomApiResponse<SysRoleResp>.Fail(GlobalConstVars.DataIsNo);

        return CustomApiResponse<SysRoleResp>.Ok(GlobalConstVars.GetDataSuccess, mapper.Map<SysRoleResp>(model));
    }

    /// <summary>
    /// 编辑角色
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> EditAsync(long id, SysRoleReq request)
    {
        var entity = await roleRepo.GetByIdAsync(id);
        if (entity is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        if (await roleRepo.IsAnyAsync(r => r.RoleName == request.RoleName && r.Id != id))
            return CustomApiResponse.Fail(GlobalConstVars.RoleNameDuplicateFailure);

        if (await roleRepo.IsAnyAsync(r => r.RoleCode == request.RoleCode && r.Id != id))
            return CustomApiResponse.Fail(GlobalConstVars.RoleCodeDuplicateFailure);

        mapper.Map(request, entity);

        var result = await roleRepo.EditAsync(entity);

        return result
            ? CustomApiResponse.Ok(GlobalConstVars.EditSuccess)
            : CustomApiResponse.Fail(GlobalConstVars.EditFailure);
    }

    /// <summary>
    /// 删除角色（存在用户关联时禁止删除）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> DeleteAsync(long id)
    {
        var entity = await roleRepo.GetByIdAsync(id);
        if (entity is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        var userRoleMaps = await userRoleRepo.GetListAsync(p => p.RoleId == entity.Id);
        if (userRoleMaps.Count > 0)
            return CustomApiResponse.Fail(GlobalConstVars.DeleteHasDependentRecords);

        await unitOfWork.BeginTranAsync();
        try
        {
            await roleMenuRepo.DeleteAsync(p => p.RoleId == id);
            var result = await roleRepo.DeleteAsync(entity.Id);
            if (!result)
            {
                await unitOfWork.RollbackTranAsync();
                return CustomApiResponse.Fail(GlobalConstVars.DeleteFailure);
            }

            await unitOfWork.CommitTranAsync();
            permissionCacheInvalidator.InvalidatePermissionCache();
            return CustomApiResponse.Ok(GlobalConstVars.DeleteSuccess);
        }
        catch
        {
            await unitOfWork.RollbackTranAsync();
            throw;
        }
    }

    /// <summary>
    /// 获取角色与菜单的映射数据（树与已勾选菜单主键）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<SysRoleMenuMapResp>> GetRoleMenuMapAsync(long id)
    {
        var role = await roleRepo.GetByIdAsync(id);
        if (role is null)
            return CustomApiResponse<SysRoleMenuMapResp>.Fail(GlobalConstVars.DataIsNo);

        var menus = await menuRepo.GetListAsync(
            p => p.Status == 1,
            p => p.Sort,
            OrderByType.Asc);
        if (menus.Count == 0)
            return CustomApiResponse<SysRoleMenuMapResp>.Fail(GlobalConstVars.RoleMenuCatalogEmptyFailure);

        var roleMenus = await roleMenuRepo.GetListAsync(p => p.RoleId == id);
        var roleMenuIds = new HashSet<long>(roleMenus.Select(rm => rm.MenuId));
        var (checkedKeys, halfCheckedKeys) = SplitRoleMenuCheckKeys(menus, roleMenuIds);

        var flat = menus
            .Select(p => new SysRoleMenuMapTreeNodeResp
            {
                Key = p.Id,
                ParentId = p.ParentId,
                Title = p.Title,
                MenuType = p.MenuType,
                Sort = p.Sort,
            })
            .ToList();

        var tree = flat.ToTree<SysRoleMenuMapTreeNodeResp, long?>(
            idSelector: x => x.Key,
            parentIdSelector: x => x.ParentId,
            sortSelector: x => x.Sort,
            childrenSelector: x => x.Children,
            rootId: null
        );

        var dto = new SysRoleMenuMapResp
        {
            TreeData = tree,
            CheckedKeys = checkedKeys,
            HalfCheckedKeys = halfCheckedKeys,
        };
        return CustomApiResponse<SysRoleMenuMapResp>.Ok(GlobalConstVars.GetDataSuccess, dto);
    }

    /// <summary>
    /// 设置角色与菜单的映射关系
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="menuIds"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> SetRoleMenuMapAsync(long roleId, List<string> menuIds)
    {
        var entity = await roleRepo.GetByIdAsync(roleId);
        if (entity is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        var distinctMenuIds = new List<long>();
        foreach (var s in (menuIds ?? []).Distinct())
        {
            if (!long.TryParse(s, out var mid))
                return CustomApiResponse.Fail(GlobalConstVars.RoleMenuInvalidMenuFailure);
            distinctMenuIds.Add(mid);
        }
        if (distinctMenuIds.Count > 0)
        {
            var existingMenus = await menuRepo.GetByIdsAsync(distinctMenuIds);
            if (existingMenus.Count != distinctMenuIds.Count
                || existingMenus.Any(p => p.Status != 1))
                return CustomApiResponse.Fail(GlobalConstVars.RoleMenuInvalidMenuFailure);
        }

        await unitOfWork.BeginTranAsync();
        try
        {
            await roleMenuRepo.DeleteAsync(p => p.RoleId == entity.Id);
            if (distinctMenuIds.Count > 0)
            {
                var list = distinctMenuIds.Select(menuId => new SysRoleMenu
                {
                    MenuId = menuId,
                    RoleId = entity.Id,
                }).ToList();

                var affected = await roleMenuRepo.InsertRangeAsync(list);
                if (affected <= 0)
                {
                    await unitOfWork.RollbackTranAsync();
                    return CustomApiResponse.Fail(GlobalConstVars.SetDataFailure);
                }
            }

            await unitOfWork.CommitTranAsync();
            permissionCacheInvalidator.InvalidatePermissionCache();
            return CustomApiResponse.Ok(GlobalConstVars.SetDataSuccess);
        }
        catch
        {
            await unitOfWork.RollbackTranAsync();
            throw;
        }
    }

    /// <summary>
    /// 将角色已分配菜单 Id 拆为 Tree 全选与半选（按子孙是否全部命中）
    /// </summary>
    private static (List<long> Checked, List<long> HalfChecked) SplitRoleMenuCheckKeys(
        List<SysMenu> menus,
        HashSet<long> roleMenuIds)
    {
        if (roleMenuIds.Count == 0)
            return ([], []);

        Dictionary<long, List<long>> childrenByParent = [];
        foreach (var menu in menus)
        {
            if (menu.ParentId is null)
                continue;
            if (!childrenByParent.TryGetValue(menu.ParentId.Value, out var list))
            {
                list = [];
                childrenByParent[menu.ParentId.Value] = list;
            }
            list.Add(menu.Id);
        }

        List<long> GetDescendantIds(long parentId)
        {
            if (!childrenByParent.TryGetValue(parentId, out var children))
                return [];
            var ids = new List<long>();
            foreach (var childId in children)
            {
                ids.Add(childId);
                ids.AddRange(GetDescendantIds(childId));
            }
            return ids;
        }

        var checkedKeys = new List<long>();
        var halfCheckedKeys = new List<long>();
        foreach (var menuId in roleMenuIds)
        {
            var descendants = GetDescendantIds(menuId);
            if (descendants.Count == 0)
                checkedKeys.Add(menuId);
            else if (descendants.All(roleMenuIds.Contains))
                checkedKeys.Add(menuId);
            else
                halfCheckedKeys.Add(menuId);
        }

        return (checkedKeys, halfCheckedKeys);
    }
}
