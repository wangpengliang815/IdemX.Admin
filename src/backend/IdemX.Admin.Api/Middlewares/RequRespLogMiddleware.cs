using LogLevel = NLog.LogLevel;

namespace IdemX.Admin.Api.Middlewares
{
    public class RequRespLogMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        private readonly RequestDelegate _next = next;
        private readonly bool _enabled = configuration["Middleware:RequestResponseLog:Enabled"].ObjectToBool();

        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method)
                || HttpMethods.IsPut(context.Request.Method)
                || HttpMethods.IsPatch(context.Request.Method))
            {
                context.Request.EnableBuffering();
                using var requestReader = new StreamReader(context.Request.Body, Encoding.UTF8, false, 1024, true);
                var rawBody = await requestReader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                context.Items["RawBody"] = rawBody;
            }

            if (!_enabled)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value;
            if (string.IsNullOrEmpty(path) || !path.Contains("api", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var startTime = DateTime.Now;
            var originalBody = context.Response.Body;
            using var ms = new MemoryStream();
            context.Response.Body = ms;

            try
            {
                await _next(context);
            }
            finally
            {
                ms.Position = 0;
                var responseBody = await new StreamReader(ms).ReadToEndAsync();
                ms.Position = 0;
                await ms.CopyToAsync(originalBody);

                var logJson = $$"""
                {
                    "timestamp": "{{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}}",
                    "method": "{{context.Request.Method}}",
                    "path": "{{path}}",
                    "query": "{{context.Request.QueryString}}",
                    "status": {{context.Response.StatusCode}},
                    "duration": {{(DateTime.Now - startTime).TotalMilliseconds}},
                    "response": {{responseBody}}
                }
                """;
                // 请求响应日志内容可能过大，所以只记录到文件
                NLogUtil.WriteFileLog(LogLevel.Info, LogType.Middleware, "Middleware-RequRespLogMiddleware", logJson);

                context.Response.Body = originalBody;
            }
        }
    }
}
