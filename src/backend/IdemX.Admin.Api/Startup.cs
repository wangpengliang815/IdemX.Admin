using Hangfire;
using Hangfire.PostgreSql;

using IdemX.Admin.Api.Hangfire;

namespace IdemX.Admin.Api
{
    public class Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        public IConfiguration Configuration { get; } = configuration;

        public IWebHostEnvironment Env { get; } = env;

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Options配置绑定
            services.Configure<AliyunOptions>(Configuration.GetSection("AliyunOptions"));
            services.Configure<HangfireOptions>(Configuration.GetSection("Hangfire"));

            // 基础设施服务注册
            services.AddScoped<ISmsProvider, AliyunSmsProvider>();
            services.AddScoped<IOssProvider, AliyunOssProvider>();

            // 操作人上下文（用于审计字段）
            services.AddScoped<IOperatorContext, AspNetOperatorContext>();

            // 雪花ID生成器
            var workerId = Configuration.GetValue<ushort>("IdGenerator:WorkerId", 1);
            services.AddSingleton<IIdGenerator>(new SnowflakeIdGenerator(workerId));

            // SqlSugar配置
            services.AddSqlSugarSetup(Configuration);

            // 跨域配置
            services.AddCorsSetup(Configuration);

            // 对象映射配置
            services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(AutoMapperConfiguration).Assembly); });

            // HTTP客户端工厂
            services.AddHttpClient();

            // Swagger API文档配置
            services.AddAdminSwaggerSetup();

            // JWT认证授权配置
            services.AddAuthorizationSetup(Configuration);

            // HTTP上下文访问器
            services.AddHttpContextSetup();

            // 替换Controller激活器为默认实现
            services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());

            // 内存缓存服务
            services.AddMemoryCache();

            // 阿里云 Tair（未启用时回退内存缓存）
            services.AddTairSetup(Configuration);

            // Hangfire（Hangfire:Enabled=true 时注册存储、Worker；周期任务在 Configure 中注册）
            var hangfireEnabled = Configuration.GetValue<bool>("Hangfire:Enabled");
            if (hangfireEnabled)
            {
                var hangfireConn = Configuration.GetConnectionString("SqlConnection")
                    ?? throw new InvalidOperationException("ConnectionStrings:SqlConnection 未配置，Hangfire 依赖同一 PostgreSQL。");
                services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(hangfireConn)));
                services.AddHangfireServer();
            }

            // MVC控制器配置
            services.AddControllers(options =>
            {
                // 实体验证过滤器
                options.Filters.Add<RequiredError>();

                // Swagger剔除不需要加入api展示的列表
                options.Conventions.Add(new ApiExplorerIgnoresFilter());

                // 全局路由约定：将 [controller]/[action] token 输出统一为“小写开头驼峰”
                options.Conventions.Add(new RouteConvention());
            })
            .AddJsonOptions(options =>
            {
                // 数据格式首字母小写（驼峰命名）
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                // 忽略循环引用
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

                // 注册 DateTime 转换器（处理不可空的 DateTime）
                options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter("yyyy-MM-dd HH:mm:ss"));

                // 注册 NullableDateTime 转换器（处理 DateTime?）
                options.JsonSerializerOptions.Converters.Add(new NullableDateTimeConverter("yyyy-MM-dd HH:mm:ss"));

                // 注册 LongToString 转换器（处理 long / long? 精度丢失问题）
                options.JsonSerializerOptions.Converters.Add(new LongToStringConverter());
                options.JsonSerializerOptions.Converters.Add(new NullableLongToStringConverter());

                // 值为null则忽略该字段，Never永不忽略
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

                // 紧凑格式（不缩进）
                options.JsonSerializerOptions.WriteIndented = false;

                // 反序列化时忽略大小写
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });
        }

        /// <summary>
        /// Autofac容器配置
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModuleRegister());

            // Controller 由 Autofac 解析（主构造函数注入，不用属性注入）
            var controllerBaseType = typeof(ControllerBase);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(t => controllerBaseType.IsAssignableFrom(t) && t != controllerBaseType)
                .InstancePerLifetimeScope();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            // 异常处理中间件（必须在最前面，以捕获所有后续中间件的异常）
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // 开发环境异常页面
            }
            else
            {
                // 生产环境异常处理（API 项目返回 JSON）
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        // 按照接口契约规则：HTTP 状态码统一返回 200，错误通过业务 code 表示
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";

                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        // 记录异常日志
                        if (exception != null)
                        {
                            NLogUtil.WriteAll(NLog.LogLevel.Error, LogType.Web, "中间件异常", "中间件层捕获异常", exception);
                        }

                        var response = new CustomApiResponse
                        {
                            Code = 1,  // 使用业务失败码，而不是 HTTP 状态码
                            Msg = exception?.Message ?? "系统异常，请查看错误描述并进行解决。",
                            Data = env.IsDevelopment()
                                ? new
                                {
                                    Message = exception?.Message ?? "发生未知错误",
                                    Type = exception?.GetType().Name ?? "Unknown",
                                    StackTrace = exception?.StackTrace
                                }
                                : null  // 生产环境不返回详细信息，避免泄露敏感信息
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    });
                });
                app.UseHsts(); // HTTP严格传输安全
            }

            // 记录IP访问日志中间件
            app.UseMiddleware<IPLogMiddleware>();

            // 请求响应日志中间件
            app.UseMiddleware<RequRespLogMiddleware>();

            // 记录访问日志中间件
            app.UseMiddleware<RecordAccessLogsMiddleware>();

            // API文档中间件（Swagger + ReDoc，生产环境关闭）
            if (!Configuration["Environment:IsProd"].ObjectToBool())
            {
                app.UseSwagger();

                // SwaggerUI：提供 Swagger UI 界面（访问路径：/api-doc）
                app.UseSwaggerUI(options =>
                {
                    // 根据版本名称倒序 遍历展示
                    typeof(CustomApiVersion.ApiVersions).GetEnumNames().OrderByDescending(e => e).ToList().ForEach(
                        version =>
                        {
                            options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"Doc {version}");
                        });
                    options.RoutePrefix = "api-doc"; // Swagger UI访问路径
                });

                // ReDoc：提供 ReDoc 文档界面（访问路径：/doc）
                app.UseReDoc(options =>
                {
                    typeof(CustomApiVersion.ApiVersions).GetEnumNames().OrderByDescending(e => e).ToList().ForEach(
                       version =>
                       {
                           options.DocumentTitle = $"IdemX.Admin API Docs";
                           options.SpecUrl = $"/swagger/{version}/swagger.json";
                       });
                    options.RoutePrefix = "doc"; // ReDoc访问路径
                });
            }

            // 跨域策略
            app.UseCors(Configuration["Cors:PolicyName"] ?? "DefaultCorsPolicy");

            // 生产环境启用HTTPS重定向
            if (Configuration["Environment:IsProd"].ObjectToBool())
            {
                app.UseHttpsRedirection();
            }

            // Cookie策略
            app.UseCookiePolicy();

            // 路由中间件
            app.UseRouting();

            // 认证中间件
            app.UseAuthentication();

            // 授权中间件
            app.UseAuthorization();

            var hangfireOptions = Configuration.GetSection("Hangfire").Get<HangfireOptions>() ?? new HangfireOptions();
            if (hangfireOptions.Enabled)
            {
                // Dashboard：HTTP Basic（浏览器原生账号密码弹窗）
                var dashboard = hangfireOptions.Dashboard ?? new HangfireDashboardAuthOptions();
                if (dashboard.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(dashboard.Username) || dashboard.Password.Length == 0)
                    {
                        throw new InvalidOperationException(
                            "已启用 Hangfire Dashboard（Hangfire:Dashboard:Enabled=true），但未配置 Hangfire:Dashboard:Username 或 Password。");
                    }

                    app.UseHangfireDashboard("/hangfire", new DashboardOptions
                    {
                        Authorization = [new HangfireBasicAuthAuthorizationFilter(dashboard.Username, dashboard.Password)],
                    });
                }
            }

            // 调整UseEndpoints位置
            app.UseEndpoints(endpoints =>
            {
                // 区域路由
                endpoints.MapControllerRoute(
                    "areas",
                    "{area:exists}/{controller=Default}/{action=Index}/{id?}"
                );

                // 默认路由
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
