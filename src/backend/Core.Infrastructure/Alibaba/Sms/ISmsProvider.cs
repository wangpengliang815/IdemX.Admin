namespace Core.Infrastructure.Alibaba;

public enum SmsSendStatus
{
    Success = 0,
    Failed = 2
}

public class SmsSendResult
{
    public SmsSendStatus Status { get; set; }

    public string Message { get; set; } = string.Empty;
}

public interface ISmsProvider
{
    /// <summary>
    /// 调用阿里云发送验证码短信（不含频控、不读写缓存）
    /// </summary>
    Task<SmsSendResult> SendVerificationCodeAsync(string phoneNumber, string code);
}
