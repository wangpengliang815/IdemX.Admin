namespace Core.Infrastructure.Options;

/// <summary>
/// Hangfire 配置（appsettings 节名：Hangfire）
/// </summary>
public class HangfireOptions
{
    /// <summary>
    /// 是否启用 Hangfire（PostgreSQL 存储、后台 Worker、周期任务注册）；为 false 时不注册 Hangfire 服务。Dashboard 另由 Dashboard:Enabled 控制。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Hangfire Dashboard（/hangfire）Basic 账号密码配置。
    /// </summary>
    public HangfireDashboardAuthOptions Dashboard { get; set; } = new();

    /// <summary>
    /// 周期任务列表
    /// </summary>
    public List<RecurringJobConfigOptions> RecurringJobs { get; set; } = new();
}

/// <summary>
/// Hangfire Dashboard Basic Auth。
/// </summary>
public class HangfireDashboardAuthOptions
{
    /// <summary>
    /// 是否启用 Dashboard（仅控制 /hangfire，不影响 Worker）。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Basic 用户名（Enabled=true 时必填）。
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Basic 密码（Enabled=true 时必填）。
    /// </summary>
    public string Password { get; set; } = "";
}

/// <summary>
/// 单条 Hangfire Recurring Job：Id + 调度（NCrontab 五段：分 时 日 月 星期）。
/// </summary>
public class RecurringJobConfigOptions
{
    /// <summary>
    /// 任务 Id（与 HangfireRecurringJobsRegistrar 中注册的 Id 一致）
    /// </summary>
    public string JobId { get; set; } = "";

    /// <summary>
    /// 是否启用该周期任务
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// NCrontab 五段表达式：分 时 日 月 星期
    /// </summary>
    public string Cron { get; set; } = "0 0 * * *";

    /// <summary>
    /// IANA（如 Asia/Shanghai）或 Windows 时区 id；空则用服务器本地时区。
    /// </summary>
    public string TimeZoneId { get; set; } = "";
}
