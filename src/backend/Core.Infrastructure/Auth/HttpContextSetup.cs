namespace Core.Infrastructure.Auth;

/// <summary>
/// Http请求上下文
/// </summary>
public static class HttpContextSetup
{
    public static void AddHttpContextSetup(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IHttpContextUser, AspNetUser>();
    }
}
