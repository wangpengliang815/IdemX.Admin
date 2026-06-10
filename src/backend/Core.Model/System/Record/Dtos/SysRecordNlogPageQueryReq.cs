namespace Core.Model.System;

public class SysRecordNlogPageQueryReq : BasePageQueryReq
{
    /// <summary>
    /// 按级别精确筛，可空
    /// </summary>
    public string LogLevel { get; set; }

    /// <summary>
    /// 日志时间起，yyyy-MM-dd，可空
    /// </summary>
    public string StartTime { get; set; }

    /// <summary>
    /// 日志时间止，yyyy-MM-dd，可空
    /// </summary>
    public string EndTime { get; set; }
}
