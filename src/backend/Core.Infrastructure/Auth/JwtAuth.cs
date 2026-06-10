using Microsoft.IdentityModel.Tokens;

namespace Core.Infrastructure.Auth;

/// <summary>
/// JWT 签发与校验共用参数（单例，与路由权限无关）
/// </summary>
public class JwtSigningSettings
{
    public string Issuer { get; set; }

    public string Audience { get; set; }

    public SigningCredentials SigningCredentials { get; set; }

    public TimeSpan Expiration { get; set; }
}

/// <summary>
/// 根据 JwtSigningSettings 签发 JWT 字符串
/// </summary>
public static class JwtToken
{
    public static string BuildJwtToken(Claim[] claims, JwtSigningSettings signingSettings)
    {
        var now = DateTime.Now;
        var jwt = new JwtSecurityToken(
            issuer: signingSettings.Issuer,
            audience: signingSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(signingSettings.Expiration),
            signingCredentials: signingSettings.SigningCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
