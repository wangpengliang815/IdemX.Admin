namespace Core.Infrastructure.Alibaba;

/// <summary>
/// 阿里云短信：仅负责 Dysmsapi 发信
/// </summary>
public class AliyunSmsProvider(IOptions<AliyunOptions> aliyunOptions) : ISmsProvider
{
    private readonly SmsOptions smsOptions = aliyunOptions.Value.Sms;

    private readonly AlibabaCloud.SDK.Dysmsapi20170525.Client client =
        CreateSmsClient(aliyunOptions.Value.Sms);

    /// <summary>
    /// 验证码有效期（分钟），与短信模板参数 min 一致
    /// </summary>
    private const int CodeExpirationMinutes = 10;

    private static AlibabaCloud.SDK.Dysmsapi20170525.Client CreateSmsClient(SmsOptions smsOptions)
    {
        var config = new AlibabaCloud.OpenApiClient.Models.Config
        {
            AccessKeyId = smsOptions.AccessKeyId,
            AccessKeySecret = smsOptions.AccessKeySecret,
            Endpoint = smsOptions.Endpoint
        };

        return new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
    }

    /// <summary>
    /// 调用阿里云发送验证码短信（不含频控、不读写缓存）
    /// </summary>
    public async Task<SmsSendResult> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        var request = new SendSmsRequest
        {
            PhoneNumbers = phoneNumber,
            SignName = smsOptions.SignName,
            TemplateCode = smsOptions.TemplateCode,
            TemplateParam = $"{{\"code\":\"{code}\",\"min\":\"{CodeExpirationMinutes}\"}}"
        };

        var runtime = new RuntimeOptions();
        try
        {
            var response = await client.SendSmsWithOptionsAsync(request, runtime);

            if (response.Body.Code != "OK")
            {
                return new SmsSendResult
                {
                    Status = SmsSendStatus.Failed,
                    Message = string.IsNullOrWhiteSpace(response.Body.Message)
                        ? "验证码发送失败，请稍后重试"
                        : $"验证码发送失败：{response.Body.Message}"
                };
            }

            return new SmsSendResult
            {
                Status = SmsSendStatus.Success,
                Message = "验证码发送成功"
            };
        }
        catch (Exception ex)
        {
            return new SmsSendResult
            {
                Status = SmsSendStatus.Failed,
                Message = $"验证码发送失败：{ex.Message}"
            };
        }
    }
}
