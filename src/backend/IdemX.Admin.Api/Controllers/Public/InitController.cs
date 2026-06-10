namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 项目初始化相关接口（需登录 + IsAdmin）
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

    /// <summary>
    /// 初始化项目种子数据（默认管理员、角色、菜单及权限关联）
    /// </summary>
    [HttpPost]
    [Description("初始化项目种子数据")]
    public Task<CustomApiResponse<object>> InitProject() =>
        initService.InitProjectAsync();
}
