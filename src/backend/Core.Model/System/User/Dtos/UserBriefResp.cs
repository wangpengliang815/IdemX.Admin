namespace Core.Model.System;

public class UserBriefResp
{
    /// <summary>
    /// 用户主键，雪花 string
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 登录账号
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    /// 手机号，已脱敏
    /// </summary>
    public string Phone { get; set; }
}
