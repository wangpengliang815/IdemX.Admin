using IdemX.Admin.Api.Utility;

namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 菜单管理
/// </summary>
/// <param name="sysMenuService"></param>
[Description("菜单管理")]
public class SysMenuController(ISysMenuService sysMenuService) : AuthorizedControllerBase
{
    /// <summary>
    /// 获取一级菜单
    /// </summary>
    [HttpGet]
    [Description("获取一级菜单")]
    public Task<CustomApiResponse<List<SysMenuResp>>> GetList() =>
        sysMenuService.GetListAsync();

    /// <summary>
    /// 按父级 Id 获取直接子级菜单
    /// </summary>
    /// <param name="parentId">父级菜单 Id</param>
    [HttpGet]
    [Description("获取菜单子节点")]
    public Task<CustomApiResponse<List<SysMenuResp>>> GetTreeNodes([FromQuery] long parentId) =>
        sysMenuService.GetTreeNodesAsync(parentId);

    /// <summary>
    /// 获取某菜单下已绑定的按钮
    /// </summary>
    /// <param name="parentMenuId">父级菜单 Id</param>
    [HttpGet]
    [Description("获取菜单下已绑定按钮")]
    public Task<CustomApiResponse<List<SysMenuResp>>> GetButtons([FromQuery] long parentMenuId) =>
        sysMenuService.GetButtonsAsync(parentMenuId);

    /// <summary>
    /// 根据Id获取菜单
    /// </summary>
    /// <param name="id">菜单主键Id</param>
    [HttpGet("{id}")]
    [Description("根据Id获取菜单")]
    public Task<CustomApiResponse<SysMenuResp>> GetById(long id) =>
        sysMenuService.GetByIdAsync(id);

    /// <summary>
    /// 创建菜单
    /// </summary>
    /// <param name="request">创建请求</param>
    [HttpPost]
    [Description("创建菜单")]
    public Task<CustomApiResponse> Create([FromBody] SysMenuReq request) =>
        sysMenuService.CreateAsync(request);

    /// <summary>
    /// 编辑菜单
    /// </summary>
    /// <param name="id">菜单主键Id</param>
    /// <param name="request">可编辑字段</param>
    [HttpPost("{id}")]
    [Description("编辑菜单")]
    public Task<CustomApiResponse> Edit(long id, [FromBody] SysMenuReq request) =>
        sysMenuService.EditAsync(id, request);

    /// <summary>
    /// 删除菜单（级联删除子菜单）
    /// </summary>
    /// <param name="id">菜单主键Id</param>
    [HttpPost("{id}")]
    [Description("删除菜单")]
    public Task<CustomApiResponse> Del(long id) =>
        sysMenuService.DeleteAsync(id);

    /// <summary>
    /// 获取所有Api端点（依赖 ApiControllerScanner 扫描当前程序集，故保留在 Controller）
    /// </summary>
    [HttpGet]
    [Description("获取所有Api端点")]
    public CustomApiResponse<List<SysMenuApiEndpointResp>> GetApiEndpoints()
    {
        var data = ApiControllerScanner.GetAllControllerAndAction();
        var list = data.OrderBy(p => p.Name).SelectMany(controller =>
            {
                var controllerTitle = string.IsNullOrWhiteSpace(controller.Description)
                    ? controller.Name
                    : $"{controller.Name} / {controller.Description}";
                var controllerName = controller.Name.ToCamelCase();
                return controller.Action.Select(action => new SysMenuApiEndpointResp
                {
                    Key = $"{action.ControllerName}_{action.ActionName}",
                    ControllerTitle = controllerTitle,
                    ControllerName = controllerName,
                    ActionTitle = string.IsNullOrWhiteSpace(action.Description) ? action.Name : $"{action.Name} / {action.Description}",
                    ActionName = action.ActionName,
                    Description = action.Description ?? string.Empty,
                });
            })
            .OrderBy(p => p.ControllerName)
            .ThenBy(p => p.ActionName)
            .ToList();
        return CustomApiResponse<List<SysMenuApiEndpointResp>>.Ok(GlobalConstVars.GetDataSuccess, list);
    }

    /// <summary>
    /// 导入按钮
    /// </summary>
    /// <param name="parentMenuId">父菜单主键 Id</param>
    /// <param name="items">按钮项列表（请求体为 JSON 数组，替换该父菜单下全部按钮子菜单）</param>
    [HttpPost("{parentMenuId}")]
    [Description("导入按钮")]
    public Task<CustomApiResponse> ImportButtons(long parentMenuId, [FromBody] List<SysMenuImportButtonItemReq> items) =>
        sysMenuService.ImportButtonsAsync(parentMenuId, items);
}
