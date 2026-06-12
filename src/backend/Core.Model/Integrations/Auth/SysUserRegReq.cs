namespace Core.Model.Auth;

/// <summary>
/// 注册请求DTO
/// </summary>
public class SysUserRegReq
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string Password { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [Required]
    public string Phone { get; set; }

    /// <summary>
    /// 短信验证码
    /// </summary>
    [Required]
    public string SmsCode { get; set; }
}
