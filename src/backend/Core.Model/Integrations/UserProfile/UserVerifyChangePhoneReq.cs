namespace Core.Model.UserProfile;

public class UserVerifyChangePhoneReq
{
    /// <summary>
    /// 当前绑定手机号收到的短信验证码
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string SmsCode { get; set; }
}
