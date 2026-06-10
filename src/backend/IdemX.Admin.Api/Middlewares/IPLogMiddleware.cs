namespace IdemX.Admin.Api.Middlewares
{
    /// <summary>
    /// 中间件 - 记录完整的API请求链路（单条日志）
    /// </summary>
    public class IPLogMiddleware(RequestDelegate next, ILogger<IPLogMiddleware> logger, IConfiguration configuration)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<IPLogMiddleware> _logger = logger;
        private readonly bool _enabled = configuration["Middleware:IPLog:Enabled"].ObjectToBool();

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_enabled)
            {
                await _next(context);
                return;
            }

            var requestPath = context.Request.Path.Value;

            // 只记录API请求
            if (string.IsNullOrEmpty(requestPath) ||
                (!requestPath.Contains("api", StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // 创建请求的唯一ID（用于追踪）
            var requestId = Guid.NewGuid().ToString("N");
            context.Response.Headers["X-Request-ID"] = requestId;

            // 记录请求开始时间
            var startTime = DateTime.Now;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // 收集请求信息
            var requestInfo = new
            {
                RequestId = requestId,
                StartTime = startTime,
                ClientIp = GetClientIp(context),
                Method = context.Request.Method,
                Path = requestPath,
                QueryString = context.Request.QueryString.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                ContentType = context.Request.ContentType,
                ContentLength = context.Request.ContentLength
            };

            // 读取请求体（如果有且不是太大）
            string requestBody = await ReadRequestBodyIfNeeded(context);

            // 执行请求
            Exception exception = null;
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // 在请求结束时记录一条完整的日志
                LogCompleteRequestInfo(
                    context,
                    requestInfo,
                    requestBody,
                    stopwatch.ElapsedMilliseconds,
                    exception
                );
            }
        }

        /// <summary>
        /// 记录完整的请求信息（单条日志）
        /// </summary>
        private void LogCompleteRequestInfo(
            HttpContext context,
            object requestInfo,
            string requestBody,
            long durationMs,
            Exception exception)
        {
            try
            {
                // 构建完整的请求响应信息
                var completeLog = new
                {
                    // 请求信息
                    Request = requestInfo,

                    // 请求体（如果存在且不是太大）
                    RequestBody = requestBody?.Length > 1000 ? "[Too Large]" : requestBody,

                    // 响应信息
                    Response = new
                    {
                        StatusCode = context.Response.StatusCode,
                        ContentType = context.Response.ContentType,
                        ContentLength = context.Response.ContentLength,
                        Headers = context.Response.Headers
                            .ToDictionary(h => h.Key, h => h.Value.ToString())
                    },

                    // 性能信息
                    Performance = new
                    {
                        DurationMs = durationMs,
                        IsSlowRequest = durationMs > 3000,
                        EndTime = DateTime.Now
                    },

                    // 异常信息（如果有）
                    Exception = exception == null ? null : new
                    {
                        Type = exception.GetType().Name,
                        Message = exception.Message,
                        StackTrace = exception.StackTrace
                    },

                    // 元数据
                    LogTimestamp = DateTime.Now,
                    LogType = "CompleteRequestLog"
                };

                NLogUtil.Info(LogType.Middleware, "Middleware-IPLogMiddleware", JsonSerializer.Serialize(completeLog));
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "记录请求日志失败");
            }
        }

        /// <summary>
        /// 读取请求体（如果需要）
        /// </summary>
        private static async Task<string> ReadRequestBodyIfNeeded(HttpContext context)
        {
            try
            {
                if (context.Request.ContentLength == null ||
                    context.Request.ContentLength <= 0 ||
                    context.Request.ContentLength > 1024 * 10)
                {
                    return null;
                }

                context.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                return body;
            }
            catch (Exception ex)
            {
                NLogUtil.Error(LogType.Middleware, "IPLogMiddleware-读取请求体失败", $"读取请求体时发生异常: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 获取客户端真实IP地址
        /// </summary>
        public static string GetClientIp(HttpContext context)
        {
            try
            {
                var xForwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrEmpty(xForwardedFor))
                {
                    var ips = xForwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    return ips[0].Trim();
                }

                var xRealIp = context.Request.Headers["X-Real-IP"].ToString();
                if (!string.IsNullOrEmpty(xRealIp))
                {
                    return xRealIp.Trim();
                }

                if (context.Connection.RemoteIpAddress != null)
                {
                    return context.Connection.RemoteIpAddress.MapToIPv4().ToString();
                }

                return "Unknown";
            }
            catch
            {
                return "Error";
            }
        }
    }
}
