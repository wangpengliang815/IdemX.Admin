namespace Core.Infrastructure.Alibaba;

/// <summary>
/// Tair 基础字符串键值操作（不含业务语义；业务层自行约定 key 与 value 格式）
/// </summary>
public interface ITairProvider
{
    /// <summary>
    /// 读取字符串值；键不存在或值为空时返回 false
    /// </summary>
    bool TryGetString(string key, out string value);

    /// <summary>
    /// 写入字符串值并设置过期时间
    /// </summary>
    void SetString(string key, string value, TimeSpan expiration);

    /// <summary>
    /// 删除指定键
    /// </summary>
    void Remove(string key);
}
