namespace Core.Model.UserProfile;

public class SysUserEditInfoReq
{
    /// <summary>
    /// 性别
    /// </summary>
    public int Sex { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 微信号
    /// </summary>
    public string WechatNo { get; set; }
}
