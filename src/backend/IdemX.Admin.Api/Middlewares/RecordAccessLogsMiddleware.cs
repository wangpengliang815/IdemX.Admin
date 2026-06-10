using System.Diagnostics;

namespace IdemX.Admin.Api.Middlewares
{
    public class RecordAccessLogsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextUser _user;
        private readonly ILogger<RecordAccessLogsMiddleware> _logger;
        private readonly List<string> _ignoreApis;
        private readonly bool _enabled;

        public RecordAccessLogsMiddleware(
            RequestDelegate next,
            IHttpContextUser user,
            ILogger<RecordAccessLogsMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _user = user;
            _logger = logger;

            _enabled = configuration["Middleware:RecordAccessLogs:Enabled"].ObjectToBool();
            var ignoreConfig = configuration["Middleware:RecordAccessLogs:IgnoreApis"];
            _ignoreApis = ParseIgnoreApis(ignoreConfig);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_enabled)
            {
                await _next(context);
                return;
            }

            var api = context.Request.Path.ToString().TrimEnd('/').ToLower();

            // 检查是否是API请求
            if (!api.Contains("api"))
            {
                await _next(context);
                return;
            }

            // 检查是否在忽略列表中
            if (_ignoreApis.Contains(api))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            // 准备日志数据
            var logData = new
            {
                User = _user.UserName ?? "Anonymous",
                IP = GetClientIp(context),
                API = api,
                BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                RequestMethod = request.Method,
                Agent = request.Headers["User-Agent"].ToString(),
                RequestData = await GetRequestData(request)
            };

            // 备份响应流
            var originalBody = context.Response.Body;
            using var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // 读取响应（不记录响应体，只记录大小）
                responseStream.Position = 0;
                var responseSize = responseStream.Length;
                responseStream.Position = 0;
                await responseStream.CopyToAsync(originalBody);

                // 完整日志记录
                var completeLog = new
                {
                    logData.User,
                    logData.IP,
                    logData.API,
                    logData.BeginTime,
                    OPTime = stopwatch.ElapsedMilliseconds + "ms",
                    logData.RequestMethod,
                    RequestData = logData.RequestData.Length > 500 ?
                        logData.RequestData.Substring(0, 500) + "..." :
                        logData.RequestData,
                    logData.Agent,
                    ResponseSize = responseSize
                };

                // 使用 NLogUtil 记录到数据库
                NLogUtil.Info(LogType.Middleware, "用户访问日志", JsonSerializer.Serialize(completeLog));

                context.Response.Body = originalBody;
            }
        }

        /// <summary>
        /// 解析忽略的API配置
        /// </summary>
        private static List<string> ParseIgnoreApis(string configValue)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(configValue))
                return result;

            // 按逗号分割，去掉空格，标准化路径（确保以/开头，去掉末尾/）
            var apis = configValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var api in apis)
            {
                var normalized = api.Trim().ToLower();

                // 确保路径格式正确
                if (!normalized.StartsWith("/"))
                    normalized = "/" + normalized;

                normalized = normalized.TrimEnd('/');

                result.Add(normalized);
            }

            return result;
        }

        /// <summary>
        /// 获取请求数据
        /// </summary>
        private static async Task<string> GetRequestData(HttpRequest request)
        {
            try
            {
                if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                    request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                {
                    request.EnableBuffering();
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    request.Body.Position = 0;

                    // 限制长度
                    return body.Length > 1000 ? body.Substring(0, 1000) + "..." : body;
                }
                else if (request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) ||
                         request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    return request.QueryString.ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                NLogUtil.Error(LogType.Middleware, "RecordAccessLogsMiddleware-读取请求数据失败", $"读取请求数据时发生异常: {ex.Message}", ex);
                return "[读取请求数据失败]";
            }
        }

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        private static string GetClientIp(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].ToString();
            return string.IsNullOrEmpty(ip)
                ? context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                : ip;
        }
    }
}
