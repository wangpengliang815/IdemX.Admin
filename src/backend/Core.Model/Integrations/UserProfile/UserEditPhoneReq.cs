using System.ComponentModel.DataAnnotations;

namespace Core.Model.UserProfile;

public class UserEditPhoneReq
{
    /// <summary>
    /// 新手机号
    /// </summary>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "请输入有效的手机号")]
    public string Phone { get; set; }

    /// <summary>
    /// 当前绑定手机号收到的短信验证码
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string SmsCode { get; set; }

    /// <summary>
    /// 新手机号收到的短信验证码
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string NewPhoneSmsCode { get; set; }
}
