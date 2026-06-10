namespace Core.Model.System;

public class SysMenuImportButtonItemReq
{
    /// <summary>
    /// 控制器名，与后端路由约定一致
    /// </summary>
    [Required]
    public string ControllerName { get; set; }

    /// <summary>
    /// Action 名，与后端路由约定一致
    /// </summary>
    [Required]
    public string ActionName { get; set; }

    /// <summary>
    /// 按钮或接口说明，可空
    /// </summary>
    public string Description { get; set; }
}
