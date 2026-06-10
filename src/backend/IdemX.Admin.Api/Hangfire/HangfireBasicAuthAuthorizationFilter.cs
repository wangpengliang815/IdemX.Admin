using Hangfire.Dashboard;

using System.Security.Cryptography;

namespace IdemX.Admin.Api.Hangfire;

/// <summary>
/// Hangfire Dashboard：HTTP Basic 鉴权（浏览器原生账号密码弹窗）
/// </summary>
public sealed class HangfireBasicAuthAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string _username;
    private readonly string _password;

    public HangfireBasicAuthAuthorizationFilter(string username, string password)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _password = password ?? throw new ArgumentNullException(nameof(password));
    }

    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        var auth = http.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(http);
            return false;
        }

        try
        {
            var encoded = auth["Basic ".Length..].Trim();
            var decoded = Convert.FromBase64String(encoded);
            var pair = Encoding.UTF8.GetString(decoded);
            var idx = pair.IndexOf(':');
            if (idx < 0)
            {
                Challenge(http);
                return false;
            }

            var user = pair[..idx];
            var pass = pair[(idx + 1)..];
            if (string.Equals(user, _username, StringComparison.Ordinal)
                && PasswordEquals(pass, _password))
            {
                return true;
            }
        }
        catch
        {
            // ignore invalid auth header format
        }

        Challenge(http);
        return false;
    }

    private static void Challenge(HttpContext http)
    {
        http.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
    }

    private static bool PasswordEquals(string actual, string expected)
    {
        var a = Encoding.UTF8.GetBytes(actual);
        var e = Encoding.UTF8.GetBytes(expected);
        return a.Length == e.Length && CryptographicOperations.FixedTimeEquals(a, e);
    }
}
