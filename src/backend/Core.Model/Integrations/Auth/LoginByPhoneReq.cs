namespace Core.Model.Auth;

public class LoginByPhoneReq
{
    /// <summary>
    /// 手机号
    /// </summary>
    [Required]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// 短信验证码
    /// </summary>
    [Required]
    public string SmsCode { get; set; }
}
