namespace Core.Model.System;

public class SysUserReq
{
    /// <summary>
    /// 登录账号
    /// </summary>
    [Required]
    [StringLength(50)]
    public string UserName { get; set; }

    /// <summary>
    /// 新密码，可空表示不修改
    /// </summary>
    [StringLength(100)]
    public string Password { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [Required]
    public UserSexType Sex { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Phone { get; set; }

    /// <summary>
    /// 绑定角色 id 列表序列化串，格式由前后端约定
    /// </summary>
    [StringLength(500)]
    public string RoleIds { get; set; }
}
