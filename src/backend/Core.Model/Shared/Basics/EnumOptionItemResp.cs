namespace Core.Model.Shared;

/// <summary>
/// 通用枚举下拉项（与前端 getEnum 约定：value / label）
/// </summary>
public class EnumOptionItemResp
{
    /// <summary>
    /// 枚举整型值
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// 展示文案（Description）
    /// </summary>
    public string Label { get; set; }
}
