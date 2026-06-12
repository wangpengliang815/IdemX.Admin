using System.Text.Json.Nodes;

namespace Core.Application;

/// <summary>
/// 运维冷启动：省市区
/// </summary>
public class InitService(
    IBaseRepo<SysArea> sysAreaRepo,
    IBaseRepo<SysUser> userRepo,
    IHttpContextUser contextUser,
    IUnitOfWork unitOfWork) : IInitService
{
    /// <summary>
    /// 从 china_areas_data.json 全量重建省市区
    /// </summary>
    public async Task<CustomApiResponse<object>> InitAreasAsync(string contentRootPath)
    {
        var denied = await EnsureInitAdminAsync();
        if (denied is not null)
            return denied;

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
    /// 库中已有用户时仅管理员可执行 InitAreas
    /// </summary>
    async Task<CustomApiResponse<object>?> EnsureInitAdminAsync()
    {
        if (await userRepo.IsAnyAsync(_ => true) && !contextUser.IsAdmin)
            return CustomApiResponse<object>.Fail(GlobalConstVars.ViewPermissionDenied);

        return null;
    }
}
