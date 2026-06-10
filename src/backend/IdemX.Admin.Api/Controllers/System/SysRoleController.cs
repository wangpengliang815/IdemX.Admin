namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 角色管理
/// </summary>
/// <param name="sysRoleService"></param>
[Description("角色管理")]
public class SysRoleController(ISysRoleService sysRoleService) : AuthorizedControllerBase
{
    /// <summary>
    /// 获取所有角色列表
    /// </summary>
    [HttpGet]
    [Description("获取所有角色列表")]
    public Task<CustomApiResponse<List<SysRoleResp>>> GetList() =>
        sysRoleService.GetListAsync();

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="request">分页查询条件</param>
    [HttpPost]
    [Description("获取列表")]
    public Task<CustomApiResponse<List<SysRoleResp>>> GetPageList([FromBody] SysRolePageQueryReq request) =>
        sysRoleService.GetPageListAsync(request);

    /// <summary>
    /// 创建提交
    /// </summary>
    /// <param name="request">角色创建请求</param>
    [HttpPost]
    [Description("创建提交")]
    public Task<CustomApiResponse> Create([FromBody] SysRoleReq request) =>
        sysRoleService.CreateAsync(request);

    /// <summary>
    /// 根据 Id 获取单行数据
    /// </summary>
    /// <param name="id">角色主键 Id</param>
    [HttpGet("{id}")]
    [Description("根据Id获取单行数据")]
    public Task<CustomApiResponse<SysRoleResp>> GetById(long id) =>
        sysRoleService.GetByIdAsync(id);

    /// <summary>
    /// 编辑提交
    /// </summary>
    /// <param name="id">角色主键 Id</param>
    /// <param name="request">可编辑字段</param>
    [HttpPost("{id}")]
    [Description("编辑提交")]
    public Task<CustomApiResponse> Edit(long id, [FromBody] SysRoleReq request) =>
        sysRoleService.EditAsync(id, request);

    /// <summary>
    /// 单选删除
    /// </summary>
    /// <param name="id">角色主键 Id</param>
    [HttpPost("{id}")]
    [Description("单选删除")]
    public Task<CustomApiResponse> Del(long id) =>
        sysRoleService.DeleteAsync(id);

    /// <summary>
    /// 获取角色对应菜单权限
    /// </summary>
    /// <param name="id">角色主键 Id</param>
    [HttpGet("{id}")]
    [Description("获取角色对应菜单权限")]
    public Task<CustomApiResponse<SysRoleMenuMapResp>> GetRoleMenuMap(long id) =>
        sysRoleService.GetRoleMenuMapAsync(id);

    /// <summary>
    /// 设置角色对应菜单权限
    /// </summary>
    /// <param name="roleId">角色主键 Id</param>
    /// <param name="menuIds">菜单主键 Id 列表（请求体为 JSON 字符串数组，与树勾选 key 一致）</param>
    [HttpPost("{roleId}")]
    [Description("设置角色对应菜单权限")]
    public Task<CustomApiResponse> SetRoleMenuMap(long roleId, [FromBody] List<string> menuIds) =>
        sysRoleService.SetRoleMenuMapAsync(roleId, menuIds);
}
