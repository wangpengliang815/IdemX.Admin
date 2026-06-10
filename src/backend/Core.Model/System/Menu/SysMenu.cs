namespace Core.Model.System;

/// <summary>
/// 系统菜单
/// </summary>
[SugarTable("public.sys_menu")]
public class SysMenu : BaseEntity
{
    /// <summary>
    /// 上级 id，null 为顶级菜单
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public long? ParentId { get; set; }

    /// <summary>
    /// 路由用英文 name，唯一性由业务约定
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    /// <summary>
    /// 路由 path
    /// </summary>
    [MaxLength(255)]
    public string Path { get; set; }

    /// <summary>
    /// 前端组件路径
    /// </summary>
    [MaxLength(255)]
    public string Component { get; set; }

    /// <summary>
    /// 重定向 path
    /// </summary>
    [MaxLength(255)]
    public string Redirect { get; set; }

    /// <summary>
    /// 侧栏或菜单显示标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    /// <summary>
    /// 图标，如 Iconify 名
    /// </summary>
    [MaxLength(100)]
    public string Icon { get; set; }

    /// <summary>
    /// 同级排序，数值越小越靠前
    /// </summary>
    [Required]
    public int Sort { get; set; }

    /// <summary>
    /// 权限标识，如 admin:user:view
    /// </summary>
    [MaxLength(200)]
    public string Authority { get; set; }

    /// <summary>
    /// 可见角色，逗号分隔
    /// </summary>
    [MaxLength(500)]
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
    [MaxLength(500)]
    public string ExternalUrl { get; set; }

    /// <summary>
    /// 内嵌 iframe URL
    /// </summary>
    [MaxLength(500)]
    public string IframeUrl { get; set; }

    /// <summary>
    /// 是否 keep-alive 缓存组件
    /// </summary>
    public bool KeepAlive { get; set; } = true;

    /// <summary>
    /// 0 菜单 1 按钮
    /// </summary>
    [Required]
    public int MenuType { get; set; }

    /// <summary>
    /// 角标文案，如 new、hot
    /// </summary>
    [MaxLength(50)]
    public string Badge { get; set; }

    /// <summary>
    /// 角标类型，如 dot、badge
    /// </summary>
    [MaxLength(50)]
    public string BadgeType { get; set; }

    /// <summary>
    /// 角标 UI 变体
    /// </summary>
    [MaxLength(50)]
    public string BadgeVariants { get; set; }

    /// <summary>
    /// 高亮所用菜单 name，用于隐藏路由高亮
    /// </summary>
    [MaxLength(255)]
    public string ActiveMenu { get; set; }

    /// <summary>
    /// 面包屑父级图标
    /// </summary>
    [MaxLength(100)]
    public string BreadcrumbParentIcon { get; set; }

    /// <summary>
    /// 新窗口打开的外链
    /// </summary>
    [MaxLength(500)]
    public string Link { get; set; }

    /// <summary>
    /// 1 启用 0 禁用
    /// </summary>
    [Required]
    public int Status { get; set; }
}
