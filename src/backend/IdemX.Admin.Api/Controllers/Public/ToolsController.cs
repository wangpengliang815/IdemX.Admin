namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 常用数据接口
/// </summary>
[Description("常用数据接口")]
public class ToolsController(IToolsService toolsService) : AuthorizedControllerBase
{
    /// <summary>
    /// 省市区联动
    /// </summary>
    [HttpPost]
    [Description("省市区联动")]
    public Task<CustomApiResponse<List<SysAreaItemResp>>> GetAreaByCode([FromBody] SysAreaGetByCodeReq request) =>
        toolsService.GetAreaByCodeAsync(request);

    /// <summary>
    /// 省市区三级树（与级联选择器 options 结构一致：label / value / children）
    /// </summary>
    [HttpPost]
    [Description("省市区三级树")]
    public Task<CustomApiResponse<List<SysAreaTreeNodeResp>>> GetAreaTree() =>
        toolsService.GetAreaTreeAsync();

    /// <summary>
    /// 获取通用枚举接口（一次性返回所有枚举定义）
    /// </summary>
    [HttpGet]
    [Description("获取通用枚举接口")]
    public CustomApiResponse<Dictionary<string, List<EnumOptionItemResp>>> GetEnum() =>
        toolsService.GetEnum();
}
