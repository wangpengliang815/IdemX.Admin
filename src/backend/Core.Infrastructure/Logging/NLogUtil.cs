using NLog;

using System.ComponentModel;

using LogLevel = NLog.LogLevel;

namespace Core.Infrastructure.Loging;

public enum LogType
{
    [Description("网站")]
    Web,
    [Description("数据库")]
    DataBase,
    [Description("Api接口")]
    ApiRequest,
    [Description("中间件")]
    Middleware,
    [Description("其他")]
    Other,
    [Description("Swagger")]
    Swagger,
    [Description("定时任务")]
    Task,
    [Description("积分会员")]
    VipPoint
}

public static class NLogUtil
{
    private static readonly Logger DbLogger = LogManager.GetLogger("logdb");
    
    private static readonly Logger FileLogger = LogManager.GetLogger("logfile");

    /// <summary>
    /// 同时写入到日志到数据库和文件
    /// </summary>
    public static void WriteAll(LogLevel logLevel, LogType logType, string logTitle, string message, Exception exception = null)
    {
        WriteFileLog(logLevel, logType, logTitle, message, exception);
        WriteDbLog(logLevel, logType, logTitle, message, exception);
    }

    /// <summary>
    /// 写日志到数据库
    /// </summary>
    public static void WriteDbLog(LogLevel logLevel, LogType logType, string logTitle, string message, Exception exception = null)
    {
        var logEvent = new LogEventInfo(logLevel, DbLogger.Name, message)
        {
            Properties =
            {
                ["LogType"] = logType.ToString(),
                ["LogTitle"] = logTitle
            },
            Exception = exception
        };
        DbLogger.Log(logEvent);
    }

    /// <summary>
    /// 写日志到文件
    /// </summary>
    public static void WriteFileLog(LogLevel logLevel, LogType logType, string logTitle, string message, Exception exception = null)
    {
        var logEvent = new LogEventInfo(logLevel, FileLogger.Name, message)
        {
            Properties =
            {
                ["LogType"] = logType.ToString(),
                ["LogTitle"] = logTitle
            },
            Exception = exception
        };
        FileLogger.Log(logEvent);
    }

    // 便捷方法
    public static void Info(LogType logType, string logTitle, string message)
    {
        WriteAll(LogLevel.Info, logType, logTitle, message);
    }

    public static void Error(LogType logType, string logTitle, string message, Exception ex = null)
    {
        WriteAll(LogLevel.Error, logType, logTitle, message, ex);
    }

    public static void Warn(LogType logType, string logTitle, string message)
    {
        WriteAll(LogLevel.Warn, logType, logTitle, message);
    }

    public static void Debug(LogType logType, string logTitle, string message)
    {
        WriteAll(LogLevel.Debug, logType, logTitle, message);
    }
}