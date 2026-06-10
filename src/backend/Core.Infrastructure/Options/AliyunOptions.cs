namespace Core.Infrastructure.Options;

/// <summary>
/// 阿里云服务配置（appsettings 节名：AliyunOptions）
/// </summary>
public class AliyunOptions
{
    public OssOptions Oss { get; set; } = new();

    public SmsOptions Sms { get; set; } = new();

    /// <summary>
    /// Tair（Redis 兼容）缓存
    /// </summary>
    public TairOptions Tair { get; set; } = new();
}

/// <summary>
/// OSS 对象存储
/// </summary>
public class OssOptions
{
    public string AccessKeyId { get; set; } = string.Empty;

    public string AccessKeySecret { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string BucketBindUrl { get; set; } = string.Empty;

    public string FileTypes { get; set; } = string.Empty;

    public int MaxSize { get; set; }
}

/// <summary>
/// 短信（Dysmsapi）
/// </summary>
public class SmsOptions
{
    public string AccessKeyId { get; set; } = string.Empty;

    public string AccessKeySecret { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string SignName { get; set; } = string.Empty;

    public string TemplateCode { get; set; } = string.Empty;
}

/// <summary>
/// Tair 连接配置
/// </summary>
public class TairOptions
{
    /// <summary>
    /// 是否启用；关闭时使用 MemoryTairProvider
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 连接地址
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// 端口，默认 6379
    /// </summary>
    public int Port { get; set; } = 6379;

    /// <summary>
    /// ACL 账号，未启用账号鉴权时可留空
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// 连接密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 逻辑库索引，默认 0
    /// </summary>
    public int Database { get; set; }

    /// <summary>
    /// 生成 StackExchange.Redis 连接串
    /// </summary>
    public string BuildConnectionString()
    {
        var endpoint = $"{Host.Trim()}:{Port}";
        var parts = new List<string> { endpoint, "abortConnect=false", "connectTimeout=5000" };

        if (!string.IsNullOrWhiteSpace(User))
            parts.Add($"user={User.Trim()}");

        if (!string.IsNullOrWhiteSpace(Password))
            parts.Add($"password={Password}");

        if (Database > 0)
            parts.Add($"defaultDatabase={Database}");

        return string.Join(',', parts);
    }
}
