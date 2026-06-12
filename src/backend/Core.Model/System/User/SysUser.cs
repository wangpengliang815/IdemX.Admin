namespace Core.Model.System;

/// <summary>
/// 系统用户主表
/// </summary>
[SugarTable("public.sys_user")]
public class SysUser : BaseEntity
{
    /// <summary>
    /// 登录账号
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; }

    /// <summary>
    /// 密码摘要，不可逆存储
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Password { get; set; }

    /// <summary>
    /// 展示用昵称
    /// </summary>
    [MaxLength(200)]
    public string NickName { get; set; }

    /// <summary>
    /// 头像 URL 或路径
    /// </summary>
    [MaxLength(200)]
    public string Avatar { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [Required]
    public UserSexType Sex { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Phone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(200)]
    public string Email { get; set; }

    /// <summary>
    /// 归属机构主键，null 表示未挂靠
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public long? SysOrgId { get; set; }

    /// <summary>
    /// 机构名称，联表或二次查询填充，不入库
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string SysOrgName { get; set; }

    /// <summary>
    /// 账号状态
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.正常;

    /// <summary>
    /// 用户类型，如注册或后台创建
    /// </summary>
    [Required]
    public UserType UserType { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    [MaxLength(18)]
    public string IdCardNumber { get; set; }

    /// <summary>
    /// 绑定角色 id 拼接串，仅展示或中间态，不入库
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string RoleIds { get; set; }

    /// <summary>
    /// 绑定角色列表，仅查询填充，不入库
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<SysRole> Roles { get; set; } = [];
}
