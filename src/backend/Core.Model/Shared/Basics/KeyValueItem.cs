namespace Core.Model.Shared;

/// <summary>
/// 通用键值结构（用于 jsonb 等强类型字段）
/// </summary>
public class KeyValueItem
{
    public string Name { get; set; }

    public string Value { get; set; }
}

