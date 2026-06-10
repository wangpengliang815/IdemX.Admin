namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 组织机构
/// </summary>
/// <param name="sysOrgService"></param>
[Description("组织机构")]
public class SysOrgController(ISysOrgService sysOrgService) : AuthorizedControllerBase
{
    /// <summary>
    /// 获取一级组织机构
    /// </summary>
    [HttpGet]
    [Description("获取一级组织机构")]
    public Task<CustomApiResponse<List<SysOrgResp>>> GetList() =>
        sysOrgService.GetListAsync();

    /// <summary>
    /// 按父级 Id 获取直接子级
    /// </summary>
    /// <param name="parentId">父级机构 Id</param>
    [HttpGet]
    [Description("获取组织机构子节点")]
    public Task<CustomApiResponse<List<SysOrgResp>>> GetTreeNodes([FromQuery] long parentId) =>
        sysOrgService.GetTreeNodesAsync(parentId);

    /// <summary>
    /// 创建提交
    /// </summary>
    /// <param name="request">创建请求</param>
    [HttpPost]
    [Description("创建提交")]
    public Task<CustomApiResponse> Create([FromBody] SysOrgReq request) =>
        sysOrgService.CreateAsync(request);

    /// <summary>
    /// 根据Id获取单行数据
    /// </summary>
    /// <param name="id">主键Id</param>
    [HttpGet("{id}")]
    [Description("根据Id获取单行数据")]
    public Task<CustomApiResponse<SysOrgResp>> GetById(long id) =>
        sysOrgService.GetByIdAsync(id);

    /// <summary>
    /// 编辑提交
    /// </summary>
    /// <param name="id">主键Id</param>
    /// <param name="request">可编辑字段</param>
    [HttpPost("{id}")]
    [Description("编辑提交")]
    public Task<CustomApiResponse> Edit(long id, [FromBody] SysOrgReq request) =>
        sysOrgService.EditAsync(id, request);

    /// <summary>
    /// 单选删除（物理删除，递归删除整个子树）
    /// </summary>
    /// <param name="id">主键Id</param>
    [HttpPost("{id}")]
    [Description("单选删除")]
    public Task<CustomApiResponse> Del(long id) =>
        sysOrgService.DeleteAsync(id);
}
