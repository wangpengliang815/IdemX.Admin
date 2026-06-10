namespace Core.Infrastructure.Utility;

/// <summary>
/// 密码摘要：新密码使用 BCrypt；登录时兼容旧 MD5 并标记是否需升级
/// </summary>
public static class PasswordHelper
{
    /// <summary>
    /// 使用 BCrypt 生成密码摘要
    /// </summary>
    public static string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword);

    /// <summary>
    /// 校验明文密码；旧 MD5 匹配时 needsUpgrade 为 true
    /// </summary>
    public static bool TryVerify(string plainPassword, string storedHash, out bool needsUpgrade)
    {
        needsUpgrade = false;

        if (IsBcryptHash(storedHash))
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);

        if (IsLegacyMd5Hash(storedHash))
        {
            var legacyHash = CommonHelper.Md5For32(plainPassword);
            if (!string.Equals(legacyHash, storedHash, StringComparison.OrdinalIgnoreCase))
                return false;

            needsUpgrade = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 是否为 BCrypt 摘要（以 $2 开头）
    /// </summary>
    public static bool IsBcryptHash(string storedHash) =>
        storedHash.StartsWith("$2", StringComparison.Ordinal);

    /// <summary>
    /// 是否为旧版 32 位十六进制 MD5 摘要
    /// </summary>
    public static bool IsLegacyMd5Hash(string storedHash) =>
        storedHash.Length == 32 && storedHash.All(static c => Uri.IsHexDigit(c));
}
