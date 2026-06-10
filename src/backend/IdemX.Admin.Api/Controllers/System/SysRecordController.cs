namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 登录日志、全局NLog日志
/// </summary>
/// <param name="sysRecordService"></param>
[Description("登录日志、全局NLog日志")]
public class SysRecordController(ISysRecordService sysRecordService) : AuthorizedControllerBase
{
    /// <summary>
    /// 登录日志-获取列表
    /// </summary>
    /// <param name="request">分页查询条件</param>
    [HttpPost]
    [Description("登录日志-获取列表")]
    public Task<CustomApiResponse<List<SysRecordLoginResp>>> GetLoginPageList([FromBody] SysRecordLoginPageQueryReq request) =>
        sysRecordService.GetLoginPageListAsync(request);

    /// <summary>
    /// 登录日志-清空数据
    /// </summary>
    [HttpPost]
    [Description("登录日志-清空数据")]
    public Task<CustomApiResponse> ClearLoginData() =>
        sysRecordService.ClearLoginDataAsync();

    /// <summary>
    /// 全局日志-获取列表
    /// </summary>
    /// <param name="request">分页查询条件</param>
    [HttpPost]
    [Description("全局日志-获取列表")]
    public Task<CustomApiResponse<List<SysRecordNlogResp>>> GetNLogPageList([FromBody] SysRecordNlogPageQueryReq request) =>
        sysRecordService.GetNLogPageListAsync(request);

    /// <summary>
    /// 全局日志-清空数据
    /// </summary>
    [HttpPost]
    [Description("全局日志-清空数据")]
    public Task<CustomApiResponse> ClearNLogData() =>
        sysRecordService.ClearNLogDataAsync();
}
