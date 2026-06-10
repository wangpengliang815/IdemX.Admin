namespace Core.Model.System;

public class SysRecordLoginResp : BaseResp
{
    /// <summary>
    /// 登录账号
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 操作系统信息
    /// </summary>
    public string Os { get; set; }

    /// <summary>
    /// 浏览器或 UA
    /// </summary>
    public string Browser { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public LoginRecordType OperType { get; set; }

    /// <summary>
    /// 备注或失败说明
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    /// 登录来源
    /// </summary>
    public string LoginSource { get; set; }
}
