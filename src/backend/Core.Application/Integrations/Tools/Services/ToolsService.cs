namespace Core.Application;

/// <summary>
/// 公共工具：省市区、枚举
/// </summary>
public class ToolsService(
    IBaseRepo<SysArea> sysAreaRepo,
    IMemoryCache cache) : IToolsService
{
    /// <summary>
    /// 按层级与父级编码查询省市区子级
    /// </summary>
    public async Task<CustomApiResponse<List<SysAreaItemResp>>> GetAreaByCodeAsync(SysAreaGetByCodeReq request)
    {
        var cacheKey = $"Areas_{request.Level}_{request.ParentCode}";

        if (cache.TryGetValue(cacheKey, out object cacheObj) && cacheObj is List<SysAreaItemResp> cachedAreas)
            return CustomApiResponse<List<SysAreaItemResp>>.Ok(GlobalConstVars.GetDataSuccess, cachedAreas);

        var areas = await sysAreaRepo.GetListAsync(p => p.Level == request.Level && p.ParentCode == request.ParentCode);
        var dto = areas
            .Select(a => new SysAreaItemResp
            {
                Code = a.Code,
                Name = a.Name,
                Level = a.Level,
                ParentCode = a.ParentCode
            })
            .ToList();

        cache.Set(cacheKey, dto, TimeSpan.FromHours(24));

        return CustomApiResponse<List<SysAreaItemResp>>.Ok(GlobalConstVars.GetDataSuccess, dto);
    }

    /// <summary>
    /// 省市区三级树（与级联 options：label / value / children 一致）
    /// </summary>
    public async Task<CustomApiResponse<List<SysAreaTreeNodeResp>>> GetAreaTreeAsync()
    {
        const string cacheKey = "Areas_Tree_L3";

        if (cache.TryGetValue(cacheKey, out object cacheObj) && cacheObj is List<SysAreaTreeNodeResp> cachedTree)
            return CustomApiResponse<List<SysAreaTreeNodeResp>>.Ok(GlobalConstVars.GetDataSuccess, cachedTree);

        var all = await sysAreaRepo.GetListAsync(
            p => p.Level >= 1 && p.Level <= 3,
            p => p.Code,
            OrderByType.Asc);

        var byParent = all
            .GroupBy(a => a.ParentCode ?? "0")
            .ToDictionary(g => g.Key, g => g.ToList());

        List<SysAreaTreeNodeResp> BuildChildren(string parentCode)
        {
            if (!byParent.TryGetValue(parentCode, out var list))
                return [];

            return [.. list
                .Select(a => new SysAreaTreeNodeResp
                {
                    Label = a.Name,
                    Value = a.Code,
                    Children = a.Level >= 3 ? null : BuildChildren(a.Code)
                })];
        }

        var tree = BuildChildren("0");
        cache.Set(cacheKey, tree, TimeSpan.FromHours(24));

        return CustomApiResponse<List<SysAreaTreeNodeResp>>.Ok(GlobalConstVars.GetDataSuccess, tree);
    }

    /// <summary>
    /// 一次性返回 Core.Model 下全部枚举选项
    /// </summary>
    public CustomApiResponse<Dictionary<string, List<EnumOptionItemResp>>> GetEnum()
    {
        var cacheKey = "AllEnums";

        if (cache.TryGetValue(cacheKey, out object cacheObj) && cacheObj is Dictionary<string, List<EnumOptionItemResp>> cachedEnums)
            return CustomApiResponse<Dictionary<string, List<EnumOptionItemResp>>>.Ok(GlobalConstVars.GetDataSuccess, cachedEnums);

        var modelAssembly = typeof(UserStatus).Assembly;
        var enumTypes = modelAssembly
            .GetTypes()
            .Where(t => t.IsEnum && t.Namespace == "Core.Model")
            .ToList();

        var allEnums = new Dictionary<string, List<EnumOptionItemResp>>();

        foreach (var enumType in enumTypes)
        {
            var enumList = EnumHelper.EnumToList(enumType);
            var options = enumList
                .Select(e => new EnumOptionItemResp { Value = e.Value, Label = e.Description ?? string.Empty })
                .ToList();

            allEnums[enumType.Name] = options;
        }

        cache.Set(cacheKey, allEnums, TimeSpan.FromHours(24));

        return CustomApiResponse<Dictionary<string, List<EnumOptionItemResp>>>.Ok(GlobalConstVars.GetDataSuccess, allEnums);
    }
}
