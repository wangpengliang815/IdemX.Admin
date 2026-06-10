namespace Core.Model.System;

public class SysRecordNlogResp
{
    /// <summary>
    /// 记录主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 日志时间
    /// </summary>
    public DateTime? LogDate { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    public string LogLevel { get; set; }

    /// <summary>
    /// 日志类型
    /// </summary>
    public string LogType { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string LogTitle { get; set; }

    /// <summary>
    /// 记录器名
    /// </summary>
    public string Logger { get; set; }

    /// <summary>
    /// 消息正文
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public string Exception { get; set; }

    /// <summary>
    /// 机器名
    /// </summary>
    public string MachineName { get; set; }

    /// <summary>
    /// 客户端 IP
    /// </summary>
    public string MachineIp { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string NetRequestMethod { get; set; }

    /// <summary>
    /// 请求地址
    /// </summary>
    public string NetRequestUrl { get; set; }

    /// <summary>
    /// 是否已认证
    /// </summary>
    public string NetUserIsauthenticated { get; set; }

    /// <summary>
    /// 认证类型
    /// </summary>
    public string NetUserAuthtype { get; set; }

    /// <summary>
    /// 用户身份摘要
    /// </summary>
    public string NetUserIdentity { get; set; }
}
