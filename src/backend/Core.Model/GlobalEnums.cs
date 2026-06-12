namespace Core.Model;

/// <summary>
/// 登录日志记录类型
/// </summary>
public enum LoginRecordType
{
    [Description("登录成功")]
    登录成功 = 0,

    [Description("登录失败")]
    登录失败 = 1,

    [Description("退出登录")]
    退出登录 = 2,

    [Description("刷新Token")]
    刷新Token = 3
}

public enum LoginSource
{
    [Description("平台登录")]
    平台登录 = 0,
}

/// <summary>
/// 用户性别
/// </summary>
public enum UserSexType
{
    [Description("男")]
    男 = 1,

    [Description("女")]
    女 = 2,

    [Description("未知")]
    未知 = 3
}

/// <summary>
/// 用户状态
/// </summary>
public enum UserStatus
{
    [Description("正常")]
    正常 = 0,

    [Description("停用")]
    停用 = 1
}

/// <summary>
/// 用户类型
/// </summary>
public enum UserType
{
    /// <summary>
    /// 内部用户
    /// </summary>
    [Description("内部用户")]
    内部用户 = 0,

    /// <summary>
    /// 注册用户
    /// </summary>
    [Description("注册用户")]
    注册用户 = 1,
}
