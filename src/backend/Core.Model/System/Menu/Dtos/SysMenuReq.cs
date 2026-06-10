namespace Core.Model.System;

public class SysMenuReq
{
    /// <summary>
    /// 上级 id，null 为顶级
    /// </summary>
    [Range(1, long.MaxValue)]
    public long? ParentId { get; set; }

    /// <summary>
    /// 路由 name，英文标识
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    /// <summary>
    /// 路由 path
    /// </summary>
    [StringLength(255)]
    public string Path { get; set; }

    /// <summary>
    /// 前端组件路径
    /// </summary>
    [StringLength(255)]
    public string Component { get; set; }

    /// <summary>
    /// 默认重定向 path
    /// </summary>
    [StringLength(255)]
    public string Redirect { get; set; }

    /// <summary>
    /// 菜单显示标题
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    /// <summary>
    /// 图标，Iconify 等
    /// </summary>
    [StringLength(100)]
    public string Icon { get; set; }

    /// <summary>
    /// 同级排序
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Sort { get; set; }

    /// <summary>
    /// 权限标识，如 admin:user:view
    /// </summary>
    [StringLength(200)]
    public string Authority { get; set; }

    /// <summary>
    /// 可见角色编码，逗号分隔
    /// </summary>
    [StringLength(500)]
    public string Roles { get; set; }

    /// <summary>
    /// 是否固定标签页
    /// </summary>
    public bool AffixTab { get; set; }

    /// <summary>
    /// 是否外链菜单
    /// </summary>
    public bool IsExternal { get; set; }

    /// <summary>
    /// 外链 URL
    /// </summary>
    [StringLength(500)]
    public string ExternalUrl { get; set; }

    /// <summary>
    /// 内嵌 iframe URL
    /// </summary>
    [StringLength(500)]
    public string IframeUrl { get; set; }

    /// <summary>
    /// 是否缓存页面组件
    /// </summary>
    public bool KeepAlive { get; set; } = true;

    /// <summary>
    /// 0 菜单 1 按钮
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MenuType { get; set; }

    /// <summary>
    /// 角标文案
    /// </summary>
    [StringLength(50)]
    public string Badge { get; set; }

    /// <summary>
    /// 角标类型，如 dot、badge
    /// </summary>
    [StringLength(50)]
    public string BadgeType { get; set; }

    /// <summary>
    /// 角标样式变体
    /// </summary>
    [StringLength(50)]
    public string BadgeVariants { get; set; }

    /// <summary>
    /// 高亮所用菜单 name
    /// </summary>
    [StringLength(255)]
    public string ActiveMenu { get; set; }

    /// <summary>
    /// 面包屑父级图标
    /// </summary>
    [StringLength(100)]
    public string BreadcrumbParentIcon { get; set; }

    /// <summary>
    /// 新窗口打开的外链
    /// </summary>
    [StringLength(500)]
    public string Link { get; set; }

    /// <summary>
    /// 1 启用 0 禁用
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Status { get; set; }
}
