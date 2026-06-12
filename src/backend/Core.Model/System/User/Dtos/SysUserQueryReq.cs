namespace Core.Model.System;

public class SysUserQueryReq
{
    /// <summary>
    /// 登录账号
    /// </summary>
    [Required]
    [StringLength(50)]
    public string UserName { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Phone { get; set; }
}
