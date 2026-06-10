namespace Core.Infrastructure.Alibaba;

using StackExchange.Redis;

/// <summary>
/// 阿里云 Tair 启动注册
/// </summary>
public static class TairSetup
{
    /// <summary>
    /// 注册 ITairProvider：Enabled 且 Host 有效时连 Tair，否则使用 MemoryTairProvider
    /// </summary>
    public static void AddTairSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var tairOptions = configuration.GetSection("AliyunOptions:Tair").Get<TairOptions>() ?? new TairOptions();

        if (tairOptions.Enabled && !string.IsNullOrWhiteSpace(tairOptions.Host))
        {
            var connectionString = tairOptions.BuildConnectionString();
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));
            services.AddSingleton<ITairProvider, AliyunTairProvider>();
            return;
        }

        services.AddSingleton<ITairProvider, MemoryTairProvider>();
    }
}
