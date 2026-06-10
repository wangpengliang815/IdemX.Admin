namespace Core.Model.System;

public class SysMenuResp : BaseResp
{
    /// <summary>
    /// 上级 id，null 为根
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// 路由 name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 路由 path
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 前端组件路径
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 重定向 path
    /// </summary>
    public string Redirect { get; set; }

    /// <summary>
    /// 显示标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 同级排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 权限标识
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    /// 可见角色串
    /// </summary>
    public string Roles { get; set; }

    /// <summary>
    /// 是否固定标签
    /// </summary>
    public bool AffixTab { get; set; }

    /// <summary>
    /// 是否外链
    /// </summary>
    public bool IsExternal { get; set; }

    /// <summary>
    /// 外链 URL
    /// </summary>
    public string ExternalUrl { get; set; }

    /// <summary>
    /// iframe URL
    /// </summary>
    public string IframeUrl { get; set; }

    /// <summary>
    /// 是否缓存页面
    /// </summary>
    public bool KeepAlive { get; set; }

    /// <summary>
    /// 0 菜单 1 按钮
    /// </summary>
    public int MenuType { get; set; }

    /// <summary>
    /// 角标文案
    /// </summary>
    public string Badge { get; set; }

    /// <summary>
    /// 角标类型
    /// </summary>
    public string BadgeType { get; set; }

    /// <summary>
    /// 角标变体
    /// </summary>
    public string BadgeVariants { get; set; }

    /// <summary>
    /// 高亮菜单 name
    /// </summary>
    public string ActiveMenu { get; set; }

    /// <summary>
    /// 面包屑父图标
    /// </summary>
    public string BreadcrumbParentIcon { get; set; }

    /// <summary>
    /// 新窗外链
    /// </summary>
    public string Link { get; set; }

    /// <summary>
    /// 1 启用 0 禁用
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 是否存在子级菜单（懒加载树用，仅统计 MenuType 为菜单的节点）
    /// </summary>
    public bool HasChild { get; set; }

    /// <summary>
    /// 子菜单（个人中心动态路由等整树返回时使用）
    /// </summary>
    public List<SysMenuResp> Children { get; set; } = [];
}
