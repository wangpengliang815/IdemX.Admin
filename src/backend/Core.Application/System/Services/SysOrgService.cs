namespace Core.Application;

public class SysOrgService(IBaseRepo<SysOrg> orgRepo
    , IBaseRepo<SysUser> userRepo
    , IUnitOfWork unitOfWork
    , IMapper mapper) : ISysOrgService
{
    /// <summary>
    /// 获取一级组织机构
    /// </summary>
    public async Task<CustomApiResponse<List<SysOrgResp>>> GetListAsync()
    {
        var nodes = await orgRepo.GetListAsync(
            p => p.ParentId == null,
            p => p.Sort,
            OrderByType.Asc);
        var dtos = await MapNodesWithHasChildAsync(nodes);
        return CustomApiResponse<List<SysOrgResp>>.Ok(GlobalConstVars.GetDataSuccess, dtos);
    }

    /// <summary>
    /// 按父级 Id 获取直接子级
    /// </summary>
    /// <param name="parentId">父级机构 Id</param>
    public async Task<CustomApiResponse<List<SysOrgResp>>> GetTreeNodesAsync(long parentId)
    {
        if (!await orgRepo.IsAnyAsync(o => o.Id == parentId))
            return CustomApiResponse<List<SysOrgResp>>.Fail(GlobalConstVars.DataIsNo);

        var nodes = await orgRepo.GetListAsync(
            p => p.ParentId == parentId,
            p => p.Sort,
            OrderByType.Asc);
        var dtos = await MapNodesWithHasChildAsync(nodes);
        return CustomApiResponse<List<SysOrgResp>>.Ok(GlobalConstVars.GetDataSuccess, dtos);
    }

    /// <summary>
    /// 新增组织机构
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> CreateAsync(SysOrgReq request)
    {
        if (request.ParentId.HasValue && !await orgRepo.IsAnyAsync(o => o.Id == request.ParentId.Value))
            return CustomApiResponse.Fail(GlobalConstVars.OrgInvalidParentFailure);

        var newId = await orgRepo.InsertAsync(mapper.Map<SysOrg>(request));
        return newId > 0
            ? CustomApiResponse.Ok(GlobalConstVars.CreateSuccess)
            : CustomApiResponse.Fail(GlobalConstVars.CreateFailure);
    }

    /// <summary>
    /// 按主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<SysOrgResp>> GetByIdAsync(long id)
    {
        var model = await orgRepo.GetByIdAsync(id);
        if (model is null)
            return CustomApiResponse<SysOrgResp>.Fail(GlobalConstVars.DataIsNo);

        return CustomApiResponse<SysOrgResp>.Ok(GlobalConstVars.GetDataSuccess, mapper.Map<SysOrgResp>(model));
    }

    /// <summary>
    /// 更新组织机构并校验上级节点
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> EditAsync(long id, SysOrgReq request)
    {
        var entity = await orgRepo.GetByIdAsync(id);
        if (entity is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
                return CustomApiResponse.Fail(GlobalConstVars.OrgSelfParentFailure);

            if (!await orgRepo.IsAnyAsync(o => o.Id == request.ParentId.Value))
                return CustomApiResponse.Fail(GlobalConstVars.OrgInvalidParentFailure);

            var allOrgs = await orgRepo.GetListAsync();
            var descendantIds = new HashSet<long>();
            var queue = new Queue<long>();
            queue.Enqueue(id);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in allOrgs.Where(o => o.ParentId == current))
                {
                    if (descendantIds.Add(child.Id))
                        queue.Enqueue(child.Id);
                }
            }

            if (descendantIds.Contains(request.ParentId.Value))
                return CustomApiResponse.Fail(GlobalConstVars.OrgChildParentFailure);
        }

        mapper.Map(request, entity);
        var result = await orgRepo.EditAsync(entity);

        return result
            ? CustomApiResponse.Ok(GlobalConstVars.EditSuccess)
            : CustomApiResponse.Fail(GlobalConstVars.EditFailure);
    }

    /// <summary>
    /// 物理删除机构及子树，子树含用户则拒绝
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> DeleteAsync(long id)
    {
        var org = await orgRepo.GetByIdAsync(id);
        if (org is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        var allOrgs = await orgRepo.GetListAsync();
        var treeIds = new HashSet<long> { id };
        var queue = new Queue<long>();
        queue.Enqueue(id);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = allOrgs
                .Where(p => p.ParentId == currentId)
                .Select(p => p.Id);

            foreach (var childId in children)
            {
                if (treeIds.Add(childId))
                {
                    queue.Enqueue(childId);
                }
            }
        }

        var hasUsers = await userRepo.IsAnyAsync(p =>
            p.SysOrgId.HasValue && treeIds.Contains(p.SysOrgId.Value));
        if (hasUsers)
            return CustomApiResponse.Fail(GlobalConstVars.DeleteHasDependentRecords);

        await unitOfWork.BeginTranAsync();
        try
        {
            var result = await orgRepo.DeleteRangeAsync(treeIds);
            if (!result)
            {
                await unitOfWork.RollbackTranAsync();
                return CustomApiResponse.Fail(GlobalConstVars.DeleteFailure);
            }

            await unitOfWork.CommitTranAsync();
            return CustomApiResponse.Ok(GlobalConstVars.DeleteSuccess);
        }
        catch
        {
            await unitOfWork.RollbackTranAsync();
            throw;
        }
    }

    /// <summary>
    /// 映射机构节点并标记是否存在子级
    /// </summary>
    private async Task<List<SysOrgResp>> MapNodesWithHasChildAsync(List<SysOrg> nodes)
    {
        if (nodes.Count == 0) return [];

        var nodeIds = nodes.Select(p => p.Id);
        var children = await orgRepo.GetListAsync(
            p => p.ParentId != null && nodeIds.Contains(p.ParentId.Value),
            p => p.Id,
            OrderByType.Asc);
        HashSet<long> parentIdsWithChildren = [.. children
            .Where(p => p.ParentId is not null)
            .Select(p => p.ParentId.Value)];

        return [.. nodes.Select(p =>
        {
            var dto = mapper.Map<SysOrgResp>(p);
            dto.HasChild = parentIdsWithChildren.Contains(p.Id);
            return dto;
        })];
    }
}
