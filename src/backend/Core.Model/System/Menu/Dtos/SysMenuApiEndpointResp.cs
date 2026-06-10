namespace Core.Model.System;

public class SysMenuApiEndpointResp
{
    /// <summary>
    /// 用于匹配的键，一般为 Controller:Action
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// 控制器显示名
    /// </summary>
    public string ControllerTitle { get; set; }

    /// <summary>
    /// 控制器类名（不含 Controller 后缀时可按项目约定）
    /// </summary>
    public string ControllerName { get; set; }

    /// <summary>
    /// Action 显示名
    /// </summary>
    public string ActionTitle { get; set; }

    /// <summary>
    /// Action 方法名
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// 接口或权限说明文案
    /// </summary>
    public string Description { get; set; }
}
