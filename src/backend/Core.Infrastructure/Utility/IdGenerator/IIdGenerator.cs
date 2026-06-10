namespace Core.Infrastructure.Utility;

/// <summary>
/// ID 生成器接口（项目自定义，简化 Yitter 接口）
/// </summary>
public interface IIdGenerator
{
    /// <summary>
    /// 生成下一个 ID
    /// </summary>
    long NextId();
}

