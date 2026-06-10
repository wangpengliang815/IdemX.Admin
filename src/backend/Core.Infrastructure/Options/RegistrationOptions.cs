namespace Core.Infrastructure.Options;

/// <summary>
/// 注册相关配置（appsettings 节名：Registration；固定种子邀请码用于冷启动）
/// </summary>
public class RegistrationOptions
{
    /// <summary>
    /// 是否启用固定种子邀请码（关闭后仅接受 ref_code 表中的邀请码）
    /// </summary>
    public bool EnableFixedSeedInviteCode { get; set; }

    /// <summary>
    /// 固定种子邀请码（启用时填写，校验前会 Trim；大小写不敏感）
    /// </summary>
    public string FixedSeedInviteCode { get; set; } = string.Empty;

    /// <summary>
    /// 注册用户默认角色编码（为空则不绑定角色）
    /// </summary>
    public string DefaultRoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 判断是否为配置中的固定种子邀请码
    /// </summary>
    public bool IsFixedSeedInviteCode(string invitationCode)
    {
        if (!EnableFixedSeedInviteCode)
            return false;

        var configured = FixedSeedInviteCode?.Trim();
        if (string.IsNullOrEmpty(configured))
            return false;

        if (string.IsNullOrWhiteSpace(invitationCode))
            return false;

        return string.Equals(
            invitationCode.Trim(),
            configured,
            StringComparison.OrdinalIgnoreCase);
    }
}
