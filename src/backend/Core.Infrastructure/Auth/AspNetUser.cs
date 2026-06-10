namespace Core.Infrastructure.Auth;

public interface IHttpContextUser
{
    string UserName { get; }

    string RealName { get; }

    string Role { get; }

    long Id { get; }

    /// <summary>
    /// 是否管理员（来自 JWT Claim，登录时按 AuthConstants.AdminRoleCode 写入）
    /// </summary>
    bool IsAdmin { get; }

    bool IsAuthenticated();

    IEnumerable<Claim> GetClaimsIdentity();

    List<string> GetClaimValueByType(string claimType);

    string GetToken();

    List<string> GetUserInfoFromToken(string claimType);
}

public class AspNetUser(IHttpContextAccessor accessor) : IHttpContextUser
{
    private readonly IHttpContextAccessor accessor = accessor;

    public string UserName => accessor.HttpContext.User.Identity.Name;

    public string RealName => accessor.HttpContext.User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.GivenName).Value;

    public string Role => accessor.HttpContext.User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Role).Value;

    public long Id
    {
        get
        {
            var idText = GetClaimValueByType(ClaimTypes.NameIdentifier).FirstOrDefault();
            if (!string.IsNullOrEmpty(idText))
                return idText.ObjectToLong();

            var legacyJti = GetClaimValueByType(JwtRegisteredClaimNames.Jti).FirstOrDefault();
            return legacyJti.ObjectToLong();
        }
    }

    public bool IsAdmin => string.Equals(
        GetClaimValueByType(AuthConstants.ClaimIsAdmin).FirstOrDefault(),
        "true",
        StringComparison.OrdinalIgnoreCase);

    public bool IsAuthenticated()
    {
        return accessor.HttpContext.User.Identity.IsAuthenticated;
    }

    public string GetToken()
    {
        return accessor.HttpContext.Request.Headers.Authorization.ObjectToString().Replace("Bearer ", "");
    }

    public List<string> GetUserInfoFromToken(string claimType)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        if (!string.IsNullOrEmpty(GetToken()))
        {
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(GetToken());

            return [.. (from item in jwtToken.Claims
                    where item.Type == claimType
                    select item.Value)];
        }
        else
        {
            return [];
        }
    }

    public IEnumerable<Claim> GetClaimsIdentity()
    {
        return accessor.HttpContext.User.Claims;
    }

    public List<string> GetClaimValueByType(string claimType)
    {
        return [.. (from item in GetClaimsIdentity()
                where item.Type == claimType
                select item.Value)];
    }
}
