namespace IdemX.Admin.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("SqlConnection");
            NLog.GlobalDiagnosticsContext.Set("ConnectionString", connectionString);

            var host = CreateHostBuilder(args).Build();

            // 测试日志
            NLogUtil.Info(LogType.Web, "网站启动", "网站启动成功");
            host.Run();
        }

        /// <summary>
        /// 创建启动支撑
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // 移除默认的日志提供程序
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog() // 使用NLog作为日志提供程序
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        // 配置连接限制和超时
                        serverOptions.Limits.MaxConcurrentConnections = 100;
                        serverOptions.Limits.MaxConcurrentUpgradedConnections = 50;
                        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
