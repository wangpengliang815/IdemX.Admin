namespace Core.Infrastructure.Alibaba;

/// <summary>
/// 进程内存键值回退（AliyunOptions.Tair.Enabled 为 false 时注册为 ITairProvider 实现）
/// </summary>
public class MemoryTairProvider(IMemoryCache memoryCache) : ITairProvider
{
    /// <summary>
    /// 从 IMemoryCache 读取字符串值；键不存在时返回 false
    /// </summary>
    public bool TryGetString(string key, out string value) =>
        memoryCache.TryGetValue(key, out value);

    /// <summary>
    /// 写入 IMemoryCache 并设置过期时间
    /// </summary>
    public void SetString(string key, string value, TimeSpan expiration) =>
        memoryCache.Set(key, value, expiration);

    /// <summary>
    /// 从 IMemoryCache 删除指定键
    /// </summary>
    public void Remove(string key) =>
        memoryCache.Remove(key);
}
