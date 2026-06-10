namespace Core.Model.Auth;

/// <summary>
/// 通过手机短信验证码重置登录密码
/// </summary>
public class ResetPasswordByPhoneReq
{
    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public string SmsCode { get; set; }

    [Required]
    public string NewPassword { get; set; }
}
