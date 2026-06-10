namespace Core.Infrastructure.Auth;

/// <summary>
/// 操作人上下文（支持 Web 和非 Web 场景）
/// </summary>
public interface IOperatorContext
{
    /// <summary>
    /// 获取当前操作人 ID
    /// </summary>
    /// <returns>Web 场景返回用户 ID，非 Web 场景返回 null</returns>
    long? GetCurrentOperatorId();

    /// <summary>
    /// 获取当前操作人用户名（username 不可变，直接从 JWT Claim 读取）
    /// </summary>
    /// <returns>Web 场景返回用户名，非 Web 场景返回 null</returns>
    string GetCurrentOperatorUsername();
}

/// <summary>
/// ASP.NET Core 操作人上下文实现
/// </summary>
public class AspNetOperatorContext(IHttpContextAccessor accessor) : IOperatorContext
{
    /// <summary>
    /// 获取当前操作人 ID
    /// </summary>
    public long? GetCurrentOperatorId()
    {
        try
        {
            if (accessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
                return null;

            var userIdText = accessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdText))
            {
                var legacyJti = accessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                userIdText = legacyJti;
            }

            return long.TryParse(userIdText, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取当前操作人用户名（从 JWT ClaimTypes.Name 读取，username 不可变）
    /// </summary>
    public string GetCurrentOperatorUsername()
    {
        try
        {
            if (accessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
                return null;

            return accessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }
        catch
        {
            return null;
        }
    }
}

