namespace IdemX.Admin.Api.Extensions
{
    /// <summary>
    /// CORS跨域配置扩展类
    /// </summary>
    public static class CorsSetup
    {
        /// <summary>
        /// 配置CORS跨域服务
        /// </summary>
        public static void AddCorsSetup(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var enableAllIps = configuration["Cors:EnableAllIPs"].ObjectToBool();
            var policyName = configuration["Cors:PolicyName"] ?? "DefaultCorsPolicy";
            var ips = configuration["Cors:IPs"] ?? string.Empty;

            services.AddCors(c =>
            {
                if (!enableAllIps)
                {
                    c.AddPolicy(policyName, policy =>
                    {
                        if (!string.IsNullOrWhiteSpace(ips))
                        {
                            policy.WithOrigins(ips.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                        }
                        policy.AllowAnyHeader();
                        policy.AllowAnyMethod();
                        policy.AllowCredentials();
                    });
                }
                else
                {
                    c.AddPolicy(policyName, policy =>
                    {
                        policy.SetIsOriginAllowed(_ => true)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
                }
            });
        }
    }
}
