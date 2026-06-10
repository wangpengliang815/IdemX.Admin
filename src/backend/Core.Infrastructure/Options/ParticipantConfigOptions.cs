namespace Core.Infrastructure.Options;

/// <summary>
/// 合约参与方默认分成配置（appsettings 节名：ParticipantConfig，数组）
/// </summary>
public class ParticipantConfigOptions
{
    /// <summary>
    /// 参与方类型（与业务枚举值一致）
    /// </summary>
    public int ParticipantType { get; set; }

    /// <summary>
    /// 展示名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 默认分成比例
    /// </summary>
    public decimal DefaultAllotPercentage { get; set; }

    /// <summary>
    /// 职责说明
    /// </summary>
    public string DutyDescription { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderBy { get; set; }
}
