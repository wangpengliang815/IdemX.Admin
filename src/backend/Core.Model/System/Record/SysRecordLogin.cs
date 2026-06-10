namespace Core.Model.System;

/// <summary>
/// 用户登录与登出审计日志
/// </summary>
[SugarTable("public.sys_record_login")]
public class SysRecordLogin : BaseEntity
{
    /// <summary>
    /// 登录账号
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; }

    /// <summary>
    /// 客户端操作系统信息
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Os { get; set; }

    /// <summary>
    /// 浏览器或 UA 简述
    /// </summary>
    public string Browser { get; set; }

    /// <summary>
    /// 记录类型，如登录成功、失败、登出
    /// </summary>
    [Required]
    public LoginRecordType OperType { get; set; }

    /// <summary>
    /// 附加说明或失败原因摘要
    /// </summary>
    [MaxLength(200)]
    public string Comments { get; set; }

    /// <summary>
    /// 登录入口，如 Web、管理端等
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string LoginSource { get; set; }
}
