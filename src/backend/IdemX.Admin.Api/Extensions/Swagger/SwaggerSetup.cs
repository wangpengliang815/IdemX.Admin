namespace IdemX.Admin.Api.Extensions
{
    public static class SwaggerSetup
    {
        // 常量定义
        private const string API_NAME = "Jyd.API 后端接口";
        private const string SUPPORT_EMAIL = "15101587969@163.com";
        private const string API_URL = "http://localhost:5000";
        private const string DOC_FILE = "IdemX.Admin.Api.xml";
        private const string MODEL_FILE = "Core.Model.xml";
        private const string SECURITY_SCHEME_NAME = "oauth2";

        public static void AddAdminSwaggerSetup(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSwaggerGen(s =>
            {
                // 遍历所有API版本
                typeof(CustomApiVersion.ApiVersions).GetEnumNames().ToList().ForEach(version =>
                {
                    s.SwaggerDoc(version, new OpenApiInfo
                    {
                        Version = version,
                        Title = $"{API_NAME} 接口文档",
                        Description = $"{API_NAME} HTTP API " + version,
                        Contact = new OpenApiContact
                        {
                            Name = API_NAME,
                            Email = SUPPORT_EMAIL,
                            Url = new Uri(API_URL)
                        },
                    });
                    s.OrderActionsBy(o => o.RelativePath);
                });

                try
                {
                    // 加载XML注释文件
                    var basePath = AppContext.BaseDirectory;
                    s.IncludeXmlComments(Path.Combine(basePath, DOC_FILE), true);
                    s.IncludeXmlComments(Path.Combine(basePath, MODEL_FILE), true);
                    s.SchemaFilter<EnumSchemaFilter>();
                }
                catch (Exception ex)
                {
                    NLogUtil.Error(LogType.Swagger, "Swagger", "XML文件加载失败", ex);
                }

                // 开启小锁图标
                s.OperationFilter<AddResponseHeadersFilter>();
                s.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                // 在header中添加token
                s.OperationFilter<SecurityRequirementsOperationFilter>();

                // 配置JWT认证
                s.AddSecurityDefinition(SECURITY_SCHEME_NAME, new OpenApiSecurityScheme
                {
                    Description = "JWT授权，格式：Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
            });
        }
    }
}
