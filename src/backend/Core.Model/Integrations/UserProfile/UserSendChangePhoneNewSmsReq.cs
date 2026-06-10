namespace Core.Model.UserProfile;

/// <summary>
/// 向换绑目标手机号发送短信验证码请求体
/// </summary>
public class UserSendChangePhoneNewSmsReq
{
    /// <summary>
    /// 换绑目标手机号
    /// </summary>
    [Required]
    [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "请输入有效的手机号")]
    public string Phone { get; set; }
}
