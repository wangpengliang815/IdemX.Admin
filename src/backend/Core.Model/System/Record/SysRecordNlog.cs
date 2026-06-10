namespace Core.Model.System;

/// <summary>
/// 应用 NLog 落库日志，与 nlog 表结构对应
/// </summary>
[SugarTable("public.sys_record_nlog")]
public class SysRecordNlog
{
    /// <summary>
    /// 自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 日志产生时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LogDate { get; set; }

    /// <summary>
    /// 级别，如 Info、Error
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string LogLevel { get; set; }

    /// <summary>
    /// 日志类型或上下文分类
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string LogType { get; set; }

    /// <summary>
    /// 日志标题
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string LogTitle { get; set; }

    /// <summary>
    /// 记录器名称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string Logger { get; set; }

    /// <summary>
    /// 正文消息
    /// </summary>
    [SugarColumn(IsNullable = true, ColumnDataType = "text")]
    public string Message { get; set; }

    /// <summary>
    /// 异常堆栈或详情
    /// </summary>
    [SugarColumn(IsNullable = true, ColumnDataType = "text")]
    public string Exception { get; set; }

    /// <summary>
    /// 主机名
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string MachineName { get; set; }

    /// <summary>
    /// 请求端 IP
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string MachineIp { get; set; }

    /// <summary>
    /// HTTP 方法
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string NetRequestMethod { get; set; }

    /// <summary>
    /// 请求 URL
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string NetRequestUrl { get; set; }

    /// <summary>
    /// 请求时是否已认证
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string NetUserIsauthenticated { get; set; }

    /// <summary>
    /// 认证方式简述
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string NetUserAuthtype { get; set; }

    /// <summary>
    /// 用户身份标识片段，如 Name 或 Claim 摘要
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string NetUserIdentity { get; set; }
}
