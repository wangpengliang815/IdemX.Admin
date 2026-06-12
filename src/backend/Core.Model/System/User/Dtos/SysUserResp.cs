namespace Core.Model.System;

public class SysUserResp : BaseResp
{
    /// <summary>
    /// 登录账号
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public UserSexType Sex { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 归属机构主键
    /// </summary>
    public long? SysOrgId { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    public string SysOrgName { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public UserStatus Status { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    public string IdCardNumber { get; set; }

    /// <summary>
    /// 已绑定角色
    /// </summary>
    public List<SysRoleResp> Roles { get; set; } = [];

    /// <summary>
    /// 是否含 admin 角色编码
    /// </summary>
    public bool IsAdmin { get; set; }
}
