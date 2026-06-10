namespace Core.Model.Auth;

public class SmsSendCodeReq
{
    [Required]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// 发码场景：login=登录（须已注册），register=注册（须未注册）
    /// </summary>
    public string Scene { get; set; }
}
