namespace Core.Application;

public class SysMenuService(IBaseRepo<SysMenu> menuRepo
    , IBaseRepo<SysRoleMenu> roleMenuRepo
    , IUnitOfWork unitOfWork
    , IPermissionCacheInvalidator permissionCacheInvalidator
    , IMapper mapper) : ISysMenuService
{
    const int MenuNodeType = 0;
    const int ButtonNodeType = 1;

    /// <summary>
    /// 获取一级菜单
    /// </summary>
    public async Task<CustomApiResponse<List<SysMenuResp>>> GetListAsync()
    {
        var nodes = await menuRepo.GetListAsync(
            p => p.ParentId == null && p.MenuType == MenuNodeType,
            p => p.Sort,
            OrderByType.Asc);
        var dtos = await MapNodesWithHasChildAsync(nodes);
        return CustomApiResponse<List<SysMenuResp>>.Ok(GlobalConstVars.GetDataSuccess, dtos);
    }

    /// <summary>
    /// 按父级 Id 获取直接子级菜单
    /// </summary>
    /// <param name="parentId">父级菜单 Id</param>
    public async Task<CustomApiResponse<List<SysMenuResp>>> GetTreeNodesAsync(long parentId)
    {
        if (!await menuRepo.IsAnyAsync(o => o.Id == parentId && o.MenuType == MenuNodeType))
            return CustomApiResponse<List<SysMenuResp>>.Fail(GlobalConstVars.DataIsNo);

        var nodes = await menuRepo.GetListAsync(
            p => p.ParentId == parentId && p.MenuType == MenuNodeType,
            p => p.Sort,
            OrderByType.Asc);
        var dtos = await MapNodesWithHasChildAsync(nodes);
        return CustomApiResponse<List<SysMenuResp>>.Ok(GlobalConstVars.GetDataSuccess, dtos);
    }

    /// <summary>
    /// 获取某菜单下已绑定的按钮
    /// </summary>
    /// <param name="parentMenuId">父级菜单 Id</param>
    public async Task<CustomApiResponse<List<SysMenuResp>>> GetButtonsAsync(long parentMenuId)
    {
        if (!await menuRepo.IsAnyAsync(o => o.Id == parentMenuId && o.MenuType == MenuNodeType))
            return CustomApiResponse<List<SysMenuResp>>.Fail(GlobalConstVars.DataIsNo);

        var buttons = await menuRepo.GetListAsync(
            p => p.ParentId == parentMenuId && p.MenuType == ButtonNodeType,
            p => p.Sort,
            OrderByType.Asc);
        var dtos = mapper.Map<List<SysMenuResp>>(buttons);
        return CustomApiResponse<List<SysMenuResp>>.Ok(GlobalConstVars.GetDataSuccess, dtos);
    }

    /// <summary>
    /// 根据Id获取菜单
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<SysMenuResp>> GetByIdAsync(long id)
    {
        var model = await menuRepo.GetByIdAsync(id);
        if (model is null)
            return CustomApiResponse<SysMenuResp>.Fail(GlobalConstVars.DataIsNo);

        return CustomApiResponse<SysMenuResp>.Ok(GlobalConstVars.GetDataSuccess, mapper.Map<SysMenuResp>(model));
    }

    /// <summary>
    /// 创建菜单
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> CreateAsync(SysMenuReq request)
    {
        if (request.ParentId.HasValue && !await menuRepo.IsAnyAsync(o => o.Id == request.ParentId.Value))
            return CustomApiResponse.Fail(GlobalConstVars.MenuInvalidParentFailure);

        var newId = await menuRepo.InsertAsync(mapper.Map<SysMenu>(request));
        return newId > 0
            ? CustomApiResponse.Ok(GlobalConstVars.CreateSuccess)
            : CustomApiResponse.Fail(GlobalConstVars.CreateFailure);
    }

    /// <summary>
    /// 编辑菜单
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> EditAsync(long id, SysMenuReq request)
    {
        var entity = await menuRepo.GetByIdAsync(id);
        if (entity is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
                return CustomApiResponse.Fail(GlobalConstVars.MenuSelfParentFailure);

            if (!await menuRepo.IsAnyAsync(o => o.Id == request.ParentId.Value))
                return CustomApiResponse.Fail(GlobalConstVars.MenuInvalidParentFailure);

            var allMenus = await menuRepo.GetListAsync();
            var descendantIds = new HashSet<long>();
            var queue = new Queue<long>();
            queue.Enqueue(id);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in allMenus.Where(o => o.ParentId == current))
                {
                    if (descendantIds.Add(child.Id))
                        queue.Enqueue(child.Id);
                }
            }

            if (descendantIds.Contains(request.ParentId.Value))
                return CustomApiResponse.Fail(GlobalConstVars.MenuChildParentFailure);
        }

        mapper.Map(request, entity);
        var result = await menuRepo.EditAsync(entity);

        return result
            ? CustomApiResponse.Ok(GlobalConstVars.EditSuccess)
            : CustomApiResponse.Fail(GlobalConstVars.EditFailure);
    }

    /// <summary>
    /// 删除菜单（级联删除子菜单及角色-菜单关联）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> DeleteAsync(long id)
    {
        var menu = await menuRepo.GetByIdAsync(id);
        if (menu is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        var allMenus = await menuRepo.GetListAsync();
        var treeIds = new HashSet<long> { id };
        var queue = new Queue<long>();
        queue.Enqueue(id);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = allMenus
                .Where(p => p.ParentId == currentId)
                .Select(p => p.Id);

            foreach (var childId in children)
            {
                if (treeIds.Add(childId))
                    queue.Enqueue(childId);
            }
        }

        await unitOfWork.BeginTranAsync();
        try
        {
            await roleMenuRepo.DeleteAsync(rm => treeIds.Contains(rm.MenuId));
            var deleted = await menuRepo.DeleteRangeAsync(treeIds);
            if (!deleted)
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
    /// 导入按钮型子菜单（替换指定父菜单下全部按钮项）
    /// </summary>
    /// <param name="parentMenuId"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> ImportButtonsAsync(long parentMenuId, List<SysMenuImportButtonItemReq> items)
    {
        var oldButtonIds = (await menuRepo.GetListAsync(
                p => p.ParentId == parentMenuId && p.MenuType == ButtonNodeType))
            .Select(p => p.Id)
            .ToList();

        await unitOfWork.BeginTranAsync();
        try
        {
            if (oldButtonIds.Count > 0)
                await roleMenuRepo.DeleteAsync(rm => oldButtonIds.Contains(rm.MenuId));

            await menuRepo.DeleteAsync(m => m.ParentId == parentMenuId && m.MenuType == ButtonNodeType);

            if (items.Count > 0)
            {
                var buttons = items.Select((p, index) => new SysMenu
                {
                    ParentId = parentMenuId,
                    Name = p.ActionName,
                    Title = string.IsNullOrWhiteSpace(p.Description) ? p.ActionName : p.Description.Trim(),
                    Component = $"/api/{p.ControllerName}/{p.ActionName}",
                    MenuType = 1,
                    Sort = index,
                    Authority = $"{p.ControllerName}:{p.ActionName}",
                    Status = 1,
                }).ToList();

                var affected = await menuRepo.InsertRangeAsync(buttons);
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
    /// 映射菜单节点并标记是否存在子级菜单
    /// </summary>
    private async Task<List<SysMenuResp>> MapNodesWithHasChildAsync(List<SysMenu> nodes)
    {
        if (nodes.Count == 0) return [];

        var nodeIds = nodes.Select(p => p.Id);
        var children = await menuRepo.GetListAsync(
            p => p.ParentId != null && nodeIds.Contains(p.ParentId.Value) && p.MenuType == MenuNodeType,
            p => p.Id,
            OrderByType.Asc);
        HashSet<long> parentIdsWithChildren = [.. children
            .Where(p => p.ParentId is not null)
            .Select(p => p.ParentId.Value)];

        return [.. nodes.Select(p =>
        {
            var dto = mapper.Map<SysMenuResp>(p);
            dto.HasChild = parentIdsWithChildren.Contains(p.Id);
            return dto;
        })];
    }
}
