namespace Core.Model.System;

public class SysRecordLoginPageQueryReq : BasePageQueryReq
{
    /// <summary>
    /// 按账号模糊筛，可空
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 登录时间起，yyyy-MM-dd，可空
    /// </summary>
    public string StartTime { get; set; }

    /// <summary>
    /// 登录时间止，yyyy-MM-dd，可空
    /// </summary>
    public string EndTime { get; set; }
}
