namespace Core.Application;

public interface ISmsCodeVerification
{
    /// <summary>
    /// 是否允许向该手机号发码（间隔与每日上限）
    /// </summary>
    bool CanSend(string phoneNumber);

    /// <summary>
    /// 发信成功后写入验证码并更新发码记录
    /// </summary>
    void SaveSentCode(string phoneNumber, string code, string purpose = null);

    /// <summary>
    /// 校验短信验证码
    /// </summary>
    /// <param name="purpose">与发码时一致；null 或空串为默认场景</param>
    /// <param name="consumeOnSuccess">校验成功后是否核销验证码</param>
    bool Verify(string phoneNumber, string code, string purpose = null, bool consumeOnSuccess = true);
}
