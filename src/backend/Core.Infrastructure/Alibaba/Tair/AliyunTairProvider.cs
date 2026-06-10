namespace Core.Infrastructure.Alibaba;

using StackExchange.Redis;

/// <summary>
/// 阿里云 Tair 基础键值服务（StackExchange.Redis，数据库索引见 AliyunOptions.Tair.Database）
/// </summary>
public class AliyunTairProvider(IConnectionMultiplexer multiplexer, IOptions<AliyunOptions> aliyunOptions) : ITairProvider
{
    private IDatabase Database => multiplexer.GetDatabase(aliyunOptions.Value.Tair.Database);

    /// <summary>
    /// 从 Tair 读取字符串值；键不存在或值为空时返回 false
    /// </summary>
    public bool TryGetString(string key, out string value)
    {
        var redisValue = Database.StringGet(key);
        if (redisValue.IsNullOrEmpty)
        {
            value = null;
            return false;
        }

        value = redisValue.ToString();
        return true;
    }

    /// <summary>
    /// 写入 Tair 并设置过期时间
    /// </summary>
    public void SetString(string key, string value, TimeSpan expiration) =>
        Database.StringSet(key, value, expiration);

    /// <summary>
    /// 从 Tair 删除指定键
    /// </summary>
    public void Remove(string key) =>
        Database.KeyDelete(key);
}
