namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 平台用户管理
/// </summary>
[Description("平台用户管理")]
public class SysUserController(ISysUserService sysUserService
    , IHttpContextUser contextUser) : AuthorizedControllerBase
{
    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="request">分页查询条件</param>
    [HttpPost]
    [Description("获取列表")]
    public Task<CustomApiResponse<List<SysUserResp>>> GetPageList([FromBody] SysUserPageQueryReq request) =>
        sysUserService.GetPageListAsync(request);

    /// <summary>
    /// 按关键字搜索用户简要信息（选人、转增等）
    /// </summary>
    [HttpGet]
    [Description("搜索用户简要信息")]
    public Task<CustomApiResponse<List<UserBriefResp>>> SearchBrief([FromQuery] string keyword) =>
        sysUserService.SearchBriefAsync(contextUser.Id, keyword);

    /// <summary>
    /// 创建提交
    /// </summary>
    /// <param name="request">用户创建请求</param>
    [HttpPost]
    [Description("创建提交")]
    public Task<CustomApiResponse> Create([FromBody] SysUserReq request) =>
        sysUserService.CreateAsync(request);

    /// <summary>
    /// 根据 Id 获取单行数据
    /// </summary>
    /// <param name="id">用户主键 Id</param>
    [HttpGet("{id}")]
    [Description("根据Id获取单行数据")]
    public Task<CustomApiResponse<SysUserResp>> GetById(long id) =>
        sysUserService.GetByIdAsync(id);

    /// <summary>
    /// 编辑提交
    /// </summary>
    /// <param name="id">用户主键 Id</param>
    /// <param name="request">可编辑字段</param>
    [HttpPost("{id}")]
    [Description("编辑提交")]
    public Task<CustomApiResponse> Edit(long id, [FromBody] SysUserReq request) =>
        sysUserService.EditAsync(id, request);

    /// <summary>
    /// 单选删除
    /// </summary>
    /// <param name="id">用户主键 Id</param>
    [HttpPost("{id}")]
    [Description("单选删除")]
    public Task<CustomApiResponse> Del(long id) =>
        sysUserService.DeleteAsync(id);

    /// <summary>
    /// 设置是否锁定
    /// </summary>
    /// <param name="id">用户主键 Id</param>
    /// <param name="status">目标状态（请求体为 JSON 数字，与 UserStatus 枚举一致）</param>
    [HttpPost("{id}")]
    [Description("设置是否锁定")]
    public Task<CustomApiResponse> SetStatus(long id, [FromBody] UserStatus status) =>
        sysUserService.SetStatusAsync(id, status);
}
