namespace Core.Model.System;

public class SysUserQueryReq
{
    /// <summary>
    /// 真实姓名（与系统用户管理一致）
    /// </summary>
    [Required]
    [StringLength(50)]
    public string RealName { get; set; }

    /// <summary>
    /// 手机号（与系统用户管理一致）
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Phone { get; set; }
}
