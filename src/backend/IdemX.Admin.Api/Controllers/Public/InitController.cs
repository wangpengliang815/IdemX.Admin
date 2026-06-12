namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 项目初始化相关接口（InitAreas 需管理员）
/// </summary>
[Description("项目初始化相关接口")]
public class InitController(IInitService initService, IWebHostEnvironment env) : AuthorizedControllerBase
{
    /// <summary>
    /// 项目初始化时初始化省市区信息
    /// </summary>
    [HttpPost]
    [Description("初始化省市区")]
    public Task<CustomApiResponse<object>> InitAreas() =>
        initService.InitAreasAsync(env.ContentRootPath);
}
